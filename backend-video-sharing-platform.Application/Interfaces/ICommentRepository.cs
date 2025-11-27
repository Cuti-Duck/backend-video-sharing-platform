using backend_video_sharing_platform.Domain.Entities;

namespace backend_video_sharing_platform.Application.Interfaces
{
    public interface ICommentRepository
    {
        Task<Comment?> GetAsync(string videoId, string commentId);
        Task SaveAsync(Comment comment);
        Task UpdateAsync(Comment comment);
        Task<List<Comment>> GetVideoCommentsAsync(string videoId);
        Task<List<Comment>> GetCommentRepliesAsync(string videoId, string parentCommentId);
        Task<int> CountVideoCommentsAsync(string videoId);
        Task DeleteAsync(string videoId, string commentId);
        Task<List<Comment>> GetAllCommentsIncludingDeleted(string videoId); // Get all without filter
    }
}