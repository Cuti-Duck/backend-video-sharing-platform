using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using Amazon.S3;
using Amazon.S3.Model;
using backend_video_sharing_platform.Application.DTOs;
using backend_video_sharing_platform.Application.Interfaces;
using Microsoft.Extensions.Configuration;

namespace backend_video_sharing_platform.Infrastructure.Services
{
    public class VideoService : IVideoService
    {
        private readonly IAmazonS3 _s3;
        private readonly IAmazonDynamoDB _dynamo;
        private readonly IConfiguration _config;
        private readonly IVideoRepository _repo;

        public VideoService(
            IAmazonS3 s3,
            IAmazonDynamoDB dynamo,
            IConfiguration config,
            IVideoRepository repo)
        {
            _s3 = s3;
            _dynamo = dynamo;
            _config = config;
            _repo = repo;
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
    }
}
