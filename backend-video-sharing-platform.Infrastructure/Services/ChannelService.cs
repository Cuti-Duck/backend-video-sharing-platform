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
                _logger.LogWarning($"Channel not found for userId {userId}.");
                return false;
            }

            if (channel.Name != newName)
            {
                channel.Name = newName;
                await _channelRepository.SaveAsync(channel);
                _logger.LogInformation($"Channel name updated for user {userId} → {newName}");
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
                    _logger.LogWarning("Channel not found: {ChannelId}", channelId);
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
                _logger.LogError(ex, "Error retrieving channel information: {ChannelId}", channelId);
                throw;
            }
        }

        public async Task<bool> UpdateDescriptionAsync(string userId, string description, CancellationToken ct = default)
        {
            var channel = await _db.LoadAsync<Channel>(userId, ct);
            if (channel == null)
            {
                _logger.LogWarning("Channel not found for userId: {UserId}", userId);
                return false;
            }

            channel.Description = description;
            await _db.SaveAsync(channel, ct);

            _logger.LogInformation("Channel description updated for userId: {UserId}", userId);
            return true;
        }

        public async Task DecreaseVideoCountAsync(string channelId)
        {
            // Load channel
            var channel = await _channelRepository.GetByUserIdAsync(channelId);
            if (channel == null)
            {
                _logger.LogWarning("Channel {ChannelId} not found when decreasing videoCount", channelId);
                return;
            }

            // Prevent negative values
            if (channel.VideoCount > 0)
                channel.VideoCount -= 1;

            await _channelRepository.SaveAsync(channel);

            _logger.LogInformation("videoCount decreased for channel {ChannelId}", channelId);
        }
    }
}
