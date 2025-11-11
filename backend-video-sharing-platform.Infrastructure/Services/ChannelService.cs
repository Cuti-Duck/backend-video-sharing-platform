using Amazon.DynamoDBv2.DataModel;
using backend_video_sharing_platform.Application.DTOs.Channel;
using backend_video_sharing_platform.Application.Interfaces;
using Microsoft.Extensions.Logging;
using backend_video_sharing_platform.Domain.Entities;


namespace backend_video_sharing_platform.Application.Services
{
    public class ChannelService : IChannelService
    {
        private readonly IChannelRepository _channelRepository;
        private readonly ILogger<ChannelService> _logger;
        private readonly IDynamoDBContext _db;

        public ChannelService(IChannelRepository channelRepository, ILogger<ChannelService> logger, IDynamoDBContext db)
        {
            _channelRepository = channelRepository;
            _logger = logger;
            _db = db;
        }

        public async Task<bool> UpdateChannelNameByUserIdAsync(string userId, string newName)
        {
            var channel = await _channelRepository.GetByUserIdAsync(userId);
            if (channel == null)
            {
                _logger.LogWarning($"Không tìm thấy Channel của userId {userId}.");
                return false;
            }

            if (channel.Name != newName)
            {
                channel.Name = newName;
                await _channelRepository.SaveAsync(channel);
                _logger.LogInformation($"Đã cập nhật tên Channel cho user {userId} → {newName}");
                return true;
            }

            return false;
        }
        public async Task<ChannelResponse?> GetChannelByIdAsync(string channelId, CancellationToken ct = default)
        {
            try
            {
                var channel = await _db.LoadAsync<Channel>(channelId, ct);

                if (channel == null)
                {
                    _logger.LogWarning("Không tìm thấy channelId: {ChannelId}", channelId);
                    return null;
                }

                return new ChannelResponse
                {
                    ChannelId = channel.ChannelId,
                    Name = channel.Name,
                    Description = channel.Description,
                    SubscriberCount = channel.SubscriberCount,
                    VideoCount = channel.VideoCount,
                    CreatedAt = channel.CreatedAt
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy thông tin channel: {ChannelId}", channelId);
                throw;
            }
        }
        public async Task<bool> UpdateDescriptionAsync(string userId, string description, CancellationToken ct = default)
        {
            var channel = await _db.LoadAsync<Channel>(userId, ct);
            if (channel == null)
            {
                _logger.LogWarning("Không tìm thấy channel cho userId: {UserId}", userId);
                return false;
            }

            channel.Description = description;
            await _db.SaveAsync(channel, ct);

            _logger.LogInformation("Đã cập nhật mô tả cho kênh của userId: {UserId}", userId);
            return true;
        }

    }
}
