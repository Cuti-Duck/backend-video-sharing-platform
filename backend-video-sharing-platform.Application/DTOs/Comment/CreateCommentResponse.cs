namespace backend_video_sharing_platform.Application.DTOs.Comment
{
    public class CreateCommentResponse
    {
        public string CommentId { get; set; } = string.Empty;
        public string VideoId { get; set; } = string.Empty;
        public string UserId { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public string? UserAvatarUrl { get; set; }
        public string Content { get; set; } = string.Empty;
        public string? ParentCommentId { get; set; }
        public int LikeCount { get; set; }
        public int ReplyCount { get; set; }
        public string CreatedAt { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
    }
}