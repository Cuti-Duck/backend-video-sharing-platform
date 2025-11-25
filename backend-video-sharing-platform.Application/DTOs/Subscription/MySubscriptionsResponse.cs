namespace backend_video_sharing_platform.Application.DTOs.Subscription
{
    public class MySubscriptionsResponse
    {
        public List<SubscribedChannelDto> Channels { get; set; } = new();
        public int TotalCount { get; set; }
    }

    public class SubscribedChannelDto
    {
        public string ChannelId { get; set; } = string.Empty;
        public string ChannelName { get; set; } = string.Empty;
        public string? AvatarUrl { get; set; }
        public int SubscriberCount { get; set; }
        public string SubscribedAt { get; set; } = string.Empty;
    }
}