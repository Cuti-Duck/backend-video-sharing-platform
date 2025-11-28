using backend_video_sharing_platform.Application.DTOs.Channel;
using backend_video_sharing_platform.Domain.Entities;

namespace backend_video_sharing_platform.Application.Interfaces
{
    public interface IChannelService
    {
        Task<bool> UpdateChannelNameByUserIdAsync(string userId, string newName);
        Task<ChannelResponse?> GetChannelByIdAsync(string channelId, CancellationToken ct = default);
        Task<bool> UpdateDescriptionAsync(string userId, string description, CancellationToken ct = default);
        Task DecreaseVideoCountAsync(string channelId);

        Task<List<Channel>> GetAllChannelsAsync();
        Task UpdateChannelAvatarAsync(string userId, string avatarUrl);
    }
}
