using backend_video_sharing_platform.Application.DTOs.Video;
using backend_video_sharing_platform.Application.DTOs.VideoLike;

namespace backend_video_sharing_platform.Application.Interfaces
{
    public interface IVideoLikeService
    {
        Task<ToggleLikeResponse> ToggleLikeAsync(string userId, string videoId);
        Task<VideoLikeStatusResponse> GetLikeStatusAsync(string userId, string videoId);
        Task<UserLikedVideosResponse> GetUserLikedVideosAsync(string userId);

    }
}
