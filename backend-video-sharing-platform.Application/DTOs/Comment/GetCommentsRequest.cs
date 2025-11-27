namespace backend_video_sharing_platform.Application.DTOs.Comment
{
    public class GetCommentsRequest
    {
        // Sorting
        public string SortBy { get; set; } = "recent"; // recent, popular, oldest

        // Pagination
        public int Limit { get; set; } = 20;
        public int Offset { get; set; } = 0;

        // Filter
        public bool IncludeReplies { get; set; } = true;
        public string? ParentCommentId { get; set; } // Nếu muốn lấy replies của 1 comment cụ thể
    }
}