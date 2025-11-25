namespace backend_video_sharing_platform.Application.DTOs.Subscription
{
    public class ChannelSubscribersResponse
    {
        public string ChannelId { get; set; } = string.Empty;
        public List<SubscriberDto> Subscribers { get; set; } = new();
        public int TotalCount { get; set; }
    }

    public class SubscriberDto
    {
        public string ChannelId { get; set; } = string.Empty;
        public string ChannelName { get; set; } = string.Empty;
        public string? AvatarUrl { get; set; }
        public string SubscribedAt { get; set; } = string.Empty;
    }
}