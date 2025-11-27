namespace backend_video_sharing_platform.Application.DTOs.Comment
{
    public class CommentLikeCountResponse
    {
        public string CommentId { get; set; } = string.Empty;
        public int LikeCount { get; set; }
    }
}
