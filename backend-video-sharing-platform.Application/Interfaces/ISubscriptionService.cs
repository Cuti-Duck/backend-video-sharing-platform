using backend_video_sharing_platform.Application.DTOs.Subscription;

namespace backend_video_sharing_platform.Application.Interfaces
{
    public interface ISubscriptionService
    {
        Task<SubscribeResponse> SubscribeAsync(string userId, SubscribeRequest request);
        Task<UnsubscribeResponse> UnsubscribeAsync(string userId, UnsubscribeRequest request);

        Task<MySubscriptionsResponse> GetMySubscriptionsAsync(string userId);
        Task<ChannelSubscribersResponse> GetChannelSubscribersAsync(string channelId);
    }
}
