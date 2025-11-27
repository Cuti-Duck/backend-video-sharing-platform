using backend_video_sharing_platform.Domain.Entities;

namespace backend_video_sharing_platform.Application.Interfaces
{
    public interface ICommentLikeRepository
    {
        Task<CommentLike?> GetAsync(string commentId, string userId);
        Task SaveAsync(CommentLike like);
        Task DeleteAsync(string commentId, string userId);
        Task<int> CountAsync(string commentId);
    }
}
