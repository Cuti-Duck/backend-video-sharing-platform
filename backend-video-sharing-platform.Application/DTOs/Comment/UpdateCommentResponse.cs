namespace backend_video_sharing_platform.Application.DTOs.Comment
{
    public class UpdateCommentResponse
    {
        public string CommentId { get; set; } = string.Empty;
        public string VideoId { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public bool IsEdited { get; set; }
        public string UpdatedAt { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
    }
}