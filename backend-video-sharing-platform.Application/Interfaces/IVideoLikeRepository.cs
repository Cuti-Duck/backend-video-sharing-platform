using backend_video_sharing_platform.Domain.Entities;

namespace backend_video_sharing_platform.Application.Interfaces
{
    public interface IVideoLikeRepository
    {
        Task<VideoLike?> GetAsync(string userId, string videoId);
        Task SaveAsync(VideoLike like);
        Task DeleteAsync(string userId, string videoId);
        Task<List<VideoLike>> GetUserLikesAsync(string userId);
        Task<int> CountVideoLikesAsync(string videoId);
    }
}
