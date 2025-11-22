using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using Amazon.S3;
using Amazon.S3.Model;
using backend_video_sharing_platform.Application.Common.Exceptions;
using backend_video_sharing_platform.Application.DTOs;
using backend_video_sharing_platform.Application.DTOs.Video;
using backend_video_sharing_platform.Application.Interfaces;
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

        public VideoService(
            IAmazonS3 s3,
            IAmazonDynamoDB dynamo,
            IConfiguration config,
            IVideoRepository repo,
            IStorageService storageService,
            ILogger<VideoService> logger
            )
        {
            _s3 = s3;
            _dynamo = dynamo;
            _config = config;
            _repo = repo;
            _storageService = storageService;
            _logger = logger;
        }

        public async Task DeleteVideoAsync(string videoId, string currentUserId)
        {
            var video = await _repo.GetByIdAsync(videoId);

            if (video == null)
                throw new NotFoundException($"Video {videoId} không tồn tại.");

            // So sánh case-insensitive và trim whitespace
            if (!string.Equals(video.UserId?.Trim(), currentUserId?.Trim(), StringComparison.OrdinalIgnoreCase))
            {
                _logger.LogWarning(
                    "FORBIDDEN - User {CurrentUserId} attempted to delete video {VideoId} owned by {VideoUserId}",
                    currentUserId, videoId, video.UserId);
                throw new ForbiddenException("Bạn không có quyền xóa video này.");
            }

            try
            {
                // Xóa file raw trong S3 nếu có
                if (!string.IsNullOrEmpty(video.Key))
                {
                    await _storageService.DeleteFileAsync(video.Key);
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Could not delete raw video file {Key}", video.Key);
                // Tiếp tục xóa dù file không tồn tại
            }

            try
            {
                // Xóa luôn thư mục processed/
                var processedPrefix = $"{video.VideoId}/";
                await _storageService.DeleteFolderAsync(processedPrefix);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Could not delete processed folder for video {VideoId}", videoId);
                // Tiếp tục xóa dù folder không tồn tại
            }

            // Xóa khỏi DynamoDB - đây là bước quan trọng nhất
            await _repo.DeleteAsync(videoId);

            _logger.LogInformation(
                "User {UserId} deleted video {VideoId} successfully",
                currentUserId, videoId);
        }

        public async Task<PresignUrlResponse> GenerateUploadUrlAsync(PresignUrlRequest request, string userId)
        {
            var rawBucket = _config["AWS:S3:RawBucket"]
                            ?? throw new InvalidOperationException("Missing AWS:S3:RawBucket config");

            var videosTable = _config["AWS:DynamoDB:VideosTable"]
                              ?? throw new InvalidOperationException("Missing AWS:DynamoDB:VideosTable config");

            // 1. Tạo videoId & S3 key
            var videoId = Guid.NewGuid().ToString();
            var key = $"raw-videos/{userId}/{videoId}.mp4";

            // 2. Generate pre-signed URL (PUT, 15 phút)
            var presignRequest = new GetPreSignedUrlRequest
            {
                BucketName = rawBucket,
                Key = key,
                Verb = HttpVerb.PUT,
                ContentType = "video/mp4",
                Expires = DateTime.UtcNow.AddMinutes(15)
            };

            var uploadUrl = _s3.GetPreSignedURL(presignRequest);

            // 3. Lưu metadata vào DynamoDB
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
                // playbackUrl, duration, thumbnailUrl sẽ được cập nhật sau bởi Lambda processed-bucket
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
                throw new NotFoundException($"Video với id '{videoId}' không tồn tại.");

            if (video.UserId != userId)
                throw new ForbiddenException("Bạn không có quyền cập nhật video này.");

            if (!contentType.StartsWith("image/"))
                throw new BadRequestException("File không hợp lệ. Chỉ chấp nhận file ảnh.");

            var ext = Path.GetExtension(fileName).ToLower();
            if (string.IsNullOrEmpty(ext))
                ext = contentType == "image/png" ? ".png" : ".jpg";

            if (ext != ".jpg" && ext != ".jpeg" && ext != ".png")
                throw new BadRequestException("Chỉ chấp nhận .jpg, .jpeg, .png.");

            var key = $"video-thumbnails/{videoId}/{DateTime.UtcNow.Ticks}{ext}";

            var url = await _storageService.UploadFileAsync(fileStream, key, contentType, ct);

            video.ThumbnailUrl = url;
            await _repo.SaveAsync(video, ct);

            _logger.LogInformation(
                "User {UserId} upload thumbnail thành công cho video {VideoId}",
                userId, videoId);
        }

        public async Task<Video> UpdateVideoAsync(string videoId, string userId, UpdateVideoRequest request)
        {
            var video = await _repo.GetByIdAsync(videoId);

            if (video == null)
                throw new NotFoundException("Video không tồn tại.");

            if (video.UserId != userId)
                throw new ForbiddenException("Bạn không có quyền cập nhật video này.");

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
                return video; // Không thay đổi gì

            video.UpdatedAt = DateTime.UtcNow.ToString("o");

            await _repo.SaveAsync(video);

            return video;
        }

    }
}
