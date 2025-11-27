namespace backend_video_sharing_platform.Application.DTOs.Video
{
    public class VideoResponse
    {
        public string VideoId { get; set; } = string.Empty;
        public string ChannelId { get; set; } = string.Empty;
        public string UserId { get; set; } = string.Empty;

        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;

        public string Status { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;

        public string? PlaybackUrl { get; set; }
        public string? ThumbnailUrl { get; set; }
        public string? Key { get; set; }

        public double Duration { get; set; }
        public long ViewCount { get; set; }
        public long LikeCount { get; set; }

        public string CreatedAt { get; set; } = string.Empty;
        public string UpdatedAt { get; set; } = string.Empty;

    }
}
