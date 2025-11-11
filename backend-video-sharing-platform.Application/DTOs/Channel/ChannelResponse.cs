namespace backend_video_sharing_platform.Application.DTOs.Channel
{
    public class ChannelResponse
    {
        public string ChannelId { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public int SubscriberCount { get; set; }
        public int VideoCount { get; set; }
        public string CreatedAt { get; set; } = string.Empty;
    }
}
