using backend_video_sharing_platform.Domain.Entities;

namespace backend_video_sharing_platform.Application.Interfaces
{
    public interface ISubscriptionRepository
    {
        Task<Subscription?> GetAsync(string userId, string channelId);
        Task SaveAsync(Subscription subscription);
        Task<int> CountSubscribersAsync(string channelId);
        Task DeleteAsync(string userId, string channelId);

        Task<List<Subscription>> GetUserSubscriptionsAsync(string userId);
        Task<List<Subscription>> GetChannelSubscribersAsync(string channelId);
    }
}
