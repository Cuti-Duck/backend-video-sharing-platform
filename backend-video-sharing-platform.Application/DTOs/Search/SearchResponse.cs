namespace backend_video_sharing_platform.Application.DTOs.Search
{
    public class SearchResponse
    {
        public string Query { get; set; } = string.Empty;
        public List<VideoSearchResult> Videos { get; set; } = new();
        public List<ChannelSearchResult> Channels { get; set; } = new();
        public int TotalVideos { get; set; }
        public int TotalChannels { get; set; }
        public int TotalResults { get; set; }
    }

    public class VideoSearchResult
    {
        public string VideoId { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string? ThumbnailUrl { get; set; }
        public string ChannelId { get; set; } = string.Empty;
        public string ChannelName { get; set; } = string.Empty;
        public long ViewCount { get; set; }
        public long LikeCount { get; set; }
        public double Duration { get; set; }
        public string CreatedAt { get; set; } = string.Empty;
    }

    public class ChannelSearchResult
    {
        public string ChannelId { get; set; } = string.Empty;
        public string ChannelName { get; set; } = string.Empty;
        public string? AvatarUrl { get; set; }
        public string? Description { get; set; }
        public int SubscriberCount { get; set; }
        public int VideoCount { get; set; }
        public string CreatedAt { get; set; } = string.Empty;
    }
}