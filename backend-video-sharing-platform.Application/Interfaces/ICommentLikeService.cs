using backend_video_sharing_platform.Application.DTOs.Comment;

namespace backend_video_sharing_platform.Application.Interfaces
{
    public interface ICommentLikeService
    {
        Task<LikeCommentResponse> ToggleLikeAsync(string videoId, string commentId, string userId);
        Task<CommentLikeCountResponse> GetLikeCountAsync(string commentId);

        Task<CommentLikeStatusResponse> CheckLikedAsync(string commentId, string userId);

    }
}
