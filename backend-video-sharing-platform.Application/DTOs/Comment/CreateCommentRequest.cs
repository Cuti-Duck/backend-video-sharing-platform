namespace backend_video_sharing_platform.Application.DTOs.Comment
{
    public class CreateCommentRequest
    {
        public string Content { get; set; } = string.Empty;

        // Null nếu là comment gốc, có giá trị nếu là reply
        public string? ParentCommentId { get; set; }
    }
}