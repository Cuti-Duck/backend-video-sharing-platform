namespace backend_video_sharing_platform.Application.DTOs.Channel
{
    public class ChannelResponse
    {
        public string ChannelId { get; set; } = string.Empty;
        public string UserId { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public int SubscriberCount { get; set; }
        public int VideoCount { get; set; }
        public string CreatedAt { get; set; } = string.Empty;

        public string? ChannelArn { get; set; }
        public string? PlaybackUrl { get; set; }
        public string? IngestEndpoint { get; set; }
        public string? StreamKeyArn { get; set; }
        public bool IsLive { get; set; } = false;
        public string? CurrentStreamId { get; set; }
        public string? AvatarUrl { get; set; }
    }
}