using backend_video_sharing_platform.Domain.Entities;

namespace backend_video_sharing_platform.Application.Interfaces
{
    public interface IChannelRepository
    {
        Task<Channel?> GetByUserIdAsync(string userId);
        Task SaveAsync(Channel channel);
        Task<Channel?> GetByIdAsync(string channelId);

    }
}
