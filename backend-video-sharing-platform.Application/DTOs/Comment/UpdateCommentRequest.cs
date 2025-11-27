namespace backend_video_sharing_platform.Application.DTOs.Comment
{
    public class UpdateCommentRequest
    {
        // Chỉ cần content - videoId lấy từ route
        public string Content { get; set; } = string.Empty;
    }
}