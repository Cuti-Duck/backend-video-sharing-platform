namespace backend_video_sharing_platform.Application.DTOs.Search
{
    public class SearchRequest
    {
        public string Query { get; set; } = string.Empty;
        public string? Type { get; set; } // video, channel, or null (both)
        public string SortBy { get; set; } = "relevance"; // relevance, views, date
        public int Limit { get; set; } = 20;
        public int Offset { get; set; } = 0;
    }
}