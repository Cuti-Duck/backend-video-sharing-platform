namespace backend_video_sharing_platform.Application.DTOs.Comment
{
    public class DeleteCommentResponse
    {
        public string CommentId { get; set; } = string.Empty;
        public string VideoId { get; set; } = string.Empty;
        public int DeletedCount { get; set; } // Số lượng comments bị xóa (bao gồm cả replies)
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
    }
}