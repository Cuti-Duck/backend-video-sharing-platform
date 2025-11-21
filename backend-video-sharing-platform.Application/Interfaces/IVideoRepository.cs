using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using backend_video_sharing_platform.Domain.Entities;

namespace backend_video_sharing_platform.Application.Interfaces
{
    public interface IVideoRepository
    {
        Task<List<Video>> GetAllVideosAsync();
        Task<List<Video>> GetVideosByChannelIdAsync(string channelId);

        Task UpdateThumbnailAsync(string videoId, string thumbnailUrl, CancellationToken ct = default);

        Task SaveAsync(Video video, CancellationToken ct = default);
        Task<Video?> GetByIdAsync(string videoId, CancellationToken ct = default);
    }
}
