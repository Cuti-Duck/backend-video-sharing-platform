namespace backend_video_sharing_platform.Application.DTOs
{
    public class VideoResponseDto
    {
        public string VideoId { get; set; }
        public string ChannelId { get; set; }
        public string UserId { get; set; }
        public string Title { get; set; }
        public string Description { get; set; } = string.Empty;
        public string PlaybackUrl { get; set; }
        public string Key { get; set; }
        public string Status { get; set; }
        public double Duration { get; set; }
        public string ThumbnailUrl { get; set; }
        public string Type { get; set; }
        public long ViewCount { get; set; }
        public long LikeCount { get; set; }
        public string CreatedAt { get; set; }

        public string UserName { get; set; } = string.Empty;
        public string UserAvatarUrl { get; set; } = string.Empty;
    }
}
