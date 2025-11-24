using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using Amazon.S3;
using Amazon.S3.Model;
using backend_video_sharing_platform.Application.Common.Exceptions;
using backend_video_sharing_platform.Application.DTOs;
using backend_video_sharing_platform.Application.DTOs.Video;
using backend_video_sharing_platform.Application.Interfaces;
using backend_video_sharing_platform.Application.Services;
using backend_video_sharing_platform.Domain.Entities;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace backend_video_sharing_platform.Infrastructure.Services
{
    public class VideoService : IVideoService
    {
        private readonly IAmazonS3 _s3;
        private readonly IAmazonDynamoDB _dynamo;
        private readonly IConfiguration _config;
        private readonly IVideoRepository _repo;
        private readonly IStorageService _storageService;
        private readonly ILogger<VideoService> _logger;
        private readonly IChannelService _channelService;

        public VideoService(
            IAmazonS3 s3,
            IAmazonDynamoDB dynamo,
            IConfiguration config,
            IVideoRepository repo,
            IStorageService storageService,
            ILogger<VideoService> logger,
            IChannelService channelService
            )
        {
            _s3 = s3;
            _dynamo = dynamo;
            _config = config;
            _repo = repo;
            _storageService = storageService;
            _logger = logger;
            _channelService = channelService;
         }

        public async Task DeleteVideoAsync(string videoId, string currentUserId)
        {
            var video = await _repo.GetByIdAsync(videoId);

            if (video == null)
                throw new NotFoundException($"Video {videoId} does not exist.");

            if (!string.Equals(video.UserId?.Trim(), currentUserId?.Trim(), StringComparison.OrdinalIgnoreCase))
                throw new ForbiddenException("You do not have permission to delete this video.");

            // Delete raw video
            try
            {
                if (!string.IsNullOrEmpty(video.Key))
                    await _storageService.DeleteFileAsync(video.Key);
            }
            catch { }

            // Delete processed files
            try
            {
                var processedPrefix = $"{video.VideoId}/";
                await _storageService.DeleteFolderAsync(processedPrefix);
            }
            catch { }

            // Delete db record
            await _repo.DeleteAsync(videoId);

            
            await _channelService.DecreaseVideoCountAsync(video.ChannelId);

            _logger.LogInformation("User {UserId} deleted video {VideoId}", currentUserId, videoId);
        }


        public async Task<PresignUrlResponse> GenerateUploadUrlAsync(PresignUrlRequest request, string userId)
        {
            var rawBucket = _config["AWS:S3:RawBucket"]
                            ?? throw new InvalidOperationException("Missing AWS:S3:RawBucket config");

            var videosTable = _config["AWS:DynamoDB:VideosTable"]
                              ?? throw new InvalidOperationException("Missing AWS:DynamoDB:VideosTable config");

            // 1. Create videoId & S3 key
            var videoId = Guid.NewGuid().ToString();
            var key = $"raw-videos/{userId}/{videoId}.mp4";

            // 2. Generate pre-signed URL (PUT, 15 minutes)
            var presignRequest = new GetPreSignedUrlRequest
            {
                BucketName = rawBucket,
                Key = key,
                Verb = HttpVerb.PUT,
                ContentType = "video/mp4",
                Expires = DateTime.UtcNow.AddMinutes(15)
            };

            var uploadUrl = _s3.GetPreSignedURL(presignRequest);

            // 3. Save metadata to DynamoDB
            var now = DateTime.UtcNow.ToString("o");

            var item = new Dictionary<string, AttributeValue>
            {
                ["videoId"] = new AttributeValue(videoId),
                ["userId"] = new AttributeValue(userId),
                ["channelId"] = new AttributeValue(request.ChannelId),
                ["title"] = new AttributeValue(request.Title),
                ["description"] = new AttributeValue(request.Description ?? string.Empty),
                ["status"] = new AttributeValue("UPLOADING"),
                ["type"] = new AttributeValue("upload"),
                ["createdAt"] = new AttributeValue(now),
                ["updatedAt"] = new AttributeValue(now),
                ["viewCount"] = new AttributeValue { N = "0" },
                ["likeCount"] = new AttributeValue { N = "0" }
                // playbackUrl, duration, thumbnailUrl will be updated later by Lambda processed-bucket
            };

            await _dynamo.PutItemAsync(new PutItemRequest
            {
                TableName = videosTable,
                Item = item
            });

            return new PresignUrlResponse
            {
                VideoId = videoId,
                UploadUrl = uploadUrl
            };
        }

        public async Task<List<VideoResponseDto>> GetAllVideosAsync()
        {
            var videos = await _repo.GetAllVideosAsync();

            return videos.Select(v => new VideoResponseDto
            {
                VideoId = v.VideoId,
                ChannelId = v.ChannelId,
                UserId = v.UserId,
                Title = v.Title,
                Description = v.Description,
                PlaybackUrl = v.PlaybackUrl,
                Key = v.Key,
                Status = v.Status,
                Duration = v.Duration,
                ThumbnailUrl = v.ThumbnailUrl,
                Type = v.Type,
                ViewCount = v.ViewCount,
                LikeCount = v.LikeCount,
                CreatedAt = v.CreatedAt
            }).ToList();
        }

        public Task<Video?> GetVideoByIdAsync(string videoId)
            => _repo.GetByIdAsync(videoId);

        public async Task<List<VideoResponseDto>> GetVideosByChannelIdAsync(string channelId)
        {
            var videos = await _repo.GetVideosByChannelIdAsync(channelId);

            return videos.Select(v => new VideoResponseDto
            {
                VideoId = v.VideoId,
                ChannelId = v.ChannelId,
                UserId = v.UserId,
                Title = v.Title,
                Description = v.Description,
                PlaybackUrl = v.PlaybackUrl,
                Key = v.Key,
                Status = v.Status,
                Duration = v.Duration,
                ThumbnailUrl = v.ThumbnailUrl,
                Type = v.Type,
                ViewCount = v.ViewCount,
                LikeCount = v.LikeCount,
                CreatedAt = v.CreatedAt
            }).ToList();
        }

        public async Task UploadThumbnailAsync(
            string videoId,
            string userId,
            Stream fileStream,
            string fileName,
            string contentType,
            CancellationToken ct = default)
        {
            var video = await _repo.GetByIdAsync(videoId, ct);
            if (video is null)
                throw new NotFoundException($"Video with id '{videoId}' does not exist.");

            if (video.UserId != userId)
                throw new ForbiddenException("You do not have permission to update this video.");

            if (!contentType.StartsWith("image/"))
                throw new BadRequestException("Invalid file. Only image files are accepted.");

            var ext = Path.GetExtension(fileName).ToLower();
            if (string.IsNullOrEmpty(ext))
                ext = contentType == "image/png" ? ".png" : ".jpg";

            if (ext != ".jpg" && ext != ".jpeg" && ext != ".png")
                throw new BadRequestException("Only .jpg, .jpeg, .png files are allowed.");

            var key = $"video-thumbnails/{videoId}/{DateTime.UtcNow.Ticks}{ext}";

            var url = await _storageService.UploadFileAsync(fileStream, key, contentType, ct);

            video.ThumbnailUrl = url;
            await _repo.SaveAsync(video, ct);

            _logger.LogInformation(
                "User {UserId} uploaded thumbnail successfully for video {VideoId}",
                userId, videoId);
        }

        public async Task<Video> UpdateVideoAsync(string videoId, string userId, UpdateVideoRequest request)
        {
            var video = await _repo.GetByIdAsync(videoId);

            if (video == null)
                throw new NotFoundException("Video does not exist.");

            if (video.UserId != userId)
                throw new ForbiddenException("You do not have permission to update this video.");

            bool isUpdated = false;

            if (request.Title != null && request.Title != video.Title)
            {
                video.Title = request.Title;
                isUpdated = true;
            }

            if (request.Description != null && request.Description != video.Description)
            {
                video.Description = request.Description;
                isUpdated = true;
            }

            if (!isUpdated)
                return video; // No changes

            video.UpdatedAt = DateTime.UtcNow.ToString("o");

            await _repo.SaveAsync(video);

            return video;
        }
    }
}
