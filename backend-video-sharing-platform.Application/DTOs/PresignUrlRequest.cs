namespace backend_video_sharing_platform.Application.DTOs
{
    public class PresignUrlRequest
    {
        public string ChannelId { get; set; } = default!;
        public string Title { get; set; }   = default!;
        public string? Description { get; set; }
    }
}
