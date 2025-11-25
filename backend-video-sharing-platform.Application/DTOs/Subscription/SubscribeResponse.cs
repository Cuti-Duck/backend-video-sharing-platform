namespace backend_video_sharing_platform.Application.DTOs.Subscription
{
    public class SubscribeResponse
    {
        public string ChannelId { get; set; } = string.Empty;
        public bool Success { get; set; }
        public int SubscriberCount { get; set; }
    }
}
