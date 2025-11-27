namespace backend_video_sharing_platform.Application.DTOs.Comment
{
    public class LikeCommentResponse
    {
        public string CommentId { get; set; } = string.Empty;
        public bool Liked { get; set; }
        public int LikeCount { get; set; }
    }
}
