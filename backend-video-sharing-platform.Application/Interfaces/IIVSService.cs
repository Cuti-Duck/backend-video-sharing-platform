namespace backend_video_sharing_platform.Application.Interfaces
{
    public interface IIVSService
    {
        Task<object> CreateChannelAsync(string userId);
        Task<IEnumerable<object>> GetVideosByUserIdAsync(string userId);
    }
}
