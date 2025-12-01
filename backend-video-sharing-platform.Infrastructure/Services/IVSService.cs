using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using Amazon.IVS;
using Amazon.IVS.Model;
using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2.DocumentModel;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using backend_video_sharing_platform.Application.DTOs.Livestream;
using backend_video_sharing_platform.Application.Interfaces;
using backend_video_sharing_platform.Domain.Entities;

// ⚠️ QUAN TRỌNG: Sử dụng ALIAS để tránh conflict
using DbChannel = backend_video_sharing_platform.Domain.Entities.Channel;
using DbStreamSession = backend_video_sharing_platform.Domain.Entities.StreamSession;

namespace backend_video_sharing_platform.Infrastructure.Services
{
    public class IVSService : IIVSService
    {
        private readonly IAmazonIVS _ivs;
        private readonly IDynamoDBContext _db;
        private readonly IConfiguration _config;
        private readonly IStorageService _storageService;

        public IVSService(
            IAmazonIVS ivs,
            IDynamoDBContext db,
            IConfiguration config,
            IStorageService storageService)
        {
            _ivs = ivs;
            _db = db;
            _config = config;
            _storageService = storageService;
        }

        public async Task<CreateLivestreamResponse> CreateLivestreamAsync(string userId)
        {
            // 1️⃣ Load channel theo userId (channelId = userId)
            var channel = await _db.LoadAsync<DbChannel>(userId);

            if (channel == null)
                throw new Exception($"Channel for userId {userId} not found. Ensure PostConfirmation trigger created it.");

            // 2️⃣ Nếu channel đã có IVS Arn → return luôn (idempotent)
            if (!string.IsNullOrEmpty(channel.ChannelArn))
            {
                var streamKeyValue = await TryGetStreamKeyAsync(channel.StreamKeyArn);

                return new CreateLivestreamResponse
                {
                    Message = "Channel already exists",
                    ChannelArn = channel.ChannelArn!,
                    PlaybackUrl = channel.PlaybackUrl!,
                    IngestServer = BuildRtmps(channel.IngestEndpoint!),
                    StreamKeyArn = channel.StreamKeyArn ?? "",
                    StreamKey = streamKeyValue ?? ""
                };
            }

            // 3️⃣ Tạo mới channel IVS
            var recordArn = _config["AWS:RecordingConfigurationArn"]
                            ?? throw new Exception("Missing RecordingConfigurationArn in appsettings.json");

            var create = await _ivs.CreateChannelAsync(new CreateChannelRequest
            {
                Name = $"user-{userId}-channel",
                Type = ChannelType.STANDARD,
                Authorized = false,
                LatencyMode = ChannelLatencyMode.LOW,
                RecordingConfigurationArn = recordArn,
                Tags = new() { { "UserId", userId }, { "Project", "VideoSharing" } }
            });

            // 4️⃣ Cập nhật lại Channel record trong DynamoDB
            channel.ChannelArn = create.Channel.Arn;
            channel.PlaybackUrl = create.Channel.PlaybackUrl;
            channel.IngestEndpoint = create.Channel.IngestEndpoint;
            channel.StreamKeyArn = create.StreamKey.Arn;

            await _db.SaveAsync(channel);

            return new CreateLivestreamResponse
            {
                Message = "Channel created successfully",
                ChannelArn = create.Channel.Arn,
                PlaybackUrl = create.Channel.PlaybackUrl,
                IngestServer = BuildRtmps(create.Channel.IngestEndpoint),
                StreamKeyArn = create.StreamKey.Arn,
                StreamKey = create.StreamKey.Value
            };
        }

        public async Task<DbStreamSession> UpdateLivestreamMetadataAsync(string userId, UpdateLivestreamMetadataRequest request)
        {
            // 1️⃣ Kiểm tra channel có tồn tại
            var channel = await _db.LoadAsync<DbChannel>(userId);
            if (channel == null)
                throw new Exception($"Channel not found for userId {userId}");

            if (string.IsNullOrEmpty(channel.ChannelArn))
                throw new Exception("Channel has not been initialized. Please call /create endpoint first.");

            // 2️⃣ Tìm hoặc tạo StreamSession PENDING
            var streamSession = await GetPendingStreamSessionAsync(userId);

            var now = DateTime.UtcNow.ToString("o");

            if (streamSession == null)
            {
                // Tạo mới StreamSession với status PENDING
                streamSession = new DbStreamSession
                {
                    StreamId = Guid.NewGuid().ToString(),
                    ChannelId = userId,
                    UserId = userId,
                    Title = request.Title,
                    Description = request.Description,
                    Status = "PENDING",
                    IsLive = 0,
                    CreatedAt = now,
                    UpdatedAt = now
                };
            }
            else
            {
                // Cập nhật metadata cho session đang pending
                streamSession.Title = request.Title;
                streamSession.Description = request.Description;
                streamSession.UpdatedAt = now;
            }

            // 3️⃣ Xử lý thumbnail - CHỈ XỬ LÝ FILE UPLOAD
            if (request.Thumbnail != null && request.Thumbnail.Length > 0)
            {
                // Upload thumbnail mới
                var thumbnailUrl = await UploadThumbnailFileAsync(userId, streamSession.StreamId, request.Thumbnail);

                // Xóa thumbnail cũ nếu có
                if (!string.IsNullOrEmpty(streamSession.ThumbnailUrl))
                {
                    await DeleteOldThumbnailAsync(streamSession.ThumbnailUrl);
                }

                // Cập nhật thumbnail URL mới
                streamSession.ThumbnailUrl = thumbnailUrl;
            }
            // Nếu không upload file mới → GIỮ NGUYÊN thumbnail cũ (hoặc null nếu chưa có)

            // 4️⃣ Lưu session
            await _db.SaveAsync(streamSession);

            return streamSession;
        }

        public async Task<DbStreamSession?> GetPendingStreamSessionAsync(string userId)
        {
            // Tạo danh sách scan conditions
            var scanConditions = new List<ScanCondition>
            {
                new ScanCondition("ChannelId", ScanOperator.Equal, userId),
                new ScanCondition("Status", ScanOperator.Equal, "PENDING")
            };

            // Pass List<ScanCondition> trực tiếp vào ScanAsync
            var search = _db.ScanAsync<DbStreamSession>(scanConditions);
            var results = await search.GetRemainingAsync();

            return results.FirstOrDefault();
        }

        // 🔧 Helper functions

        private async Task<string> UploadThumbnailFileAsync(string userId, string streamId, IFormFile thumbnail)
        {
            // Validate file
            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".webp" };
            var extension = Path.GetExtension(thumbnail.FileName).ToLowerInvariant();

            if (!allowedExtensions.Contains(extension))
                throw new ArgumentException("Only JPG, PNG, and WebP images are allowed");

            // Validate file size (max 5MB)
            if (thumbnail.Length > 5 * 1024 * 1024)
                throw new ArgumentException("Thumbnail file size must be less than 5MB");

            // Upload to S3
            // Path: livestream-thumbnails/{userId}/{streamId}/{guid}.jpg
            var fileName = $"livestream-thumbnails/{userId}/{streamId}/{Guid.NewGuid()}{extension}";

            using var stream = thumbnail.OpenReadStream();
            var thumbnailUrl = await _storageService.UploadFileAsync(
                stream,
                fileName,
                thumbnail.ContentType
            );

            return thumbnailUrl;
        }

        private async Task DeleteOldThumbnailAsync(string thumbnailUrl)
        {
            try
            {
                var key = ExtractS3KeyFromUrl(thumbnailUrl);
                if (!string.IsNullOrEmpty(key))
                {
                    await _storageService.DeleteFileAsync(key);
                }
            }
            catch (Exception ex)
            {
                // Log nhưng không throw - cho phép tiếp tục
                Console.WriteLine($"Failed to delete old thumbnail: {ex.Message}");
            }
        }

        private string? ExtractS3KeyFromUrl(string url)
        {
            try
            {
                // URL format: https://bucket-name.s3.region.amazonaws.com/key
                var uri = new Uri(url);
                return uri.AbsolutePath.TrimStart('/');
            }
            catch
            {
                return null;
            }
        }

        private static string BuildRtmps(string endpoint) =>
            $"rtmps://{endpoint}:443/app/";

        private async Task<string?> TryGetStreamKeyAsync(string? streamKeyArn)
        {
            if (string.IsNullOrEmpty(streamKeyArn)) return null;

            try
            {
                var res = await _ivs.GetStreamKeyAsync(new GetStreamKeyRequest { Arn = streamKeyArn });
                return res.StreamKey?.Value;
            }
            catch
            {
                return null;
            }
        }
    }
}