namespace backend_video_sharing_platform.Application.DTOs.Video
{
    public class TrendingVideoResponse
    {
        public string VideoId { get; set; } = string.Empty;
        public string ChannelId { get; set; } = string.Empty;
        public int CommentCount { get; set; }
        public string CreatedAt { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public int Duration { get; set; }
        public string Key { get; set; } = string.Empty;
        public int LikeCount { get; set; }
        public string PlaybackUrl { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string ThumbnailUrl { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public string UpdatedAt { get; set; } = string.Empty;
        public string UserId { get; set; } = string.Empty;
        public int ViewCount { get; set; }

        public string UserName { get; set; } = string.Empty;
        public string UserAvatarUrl { get; set; } = string.Empty;
    }
}
