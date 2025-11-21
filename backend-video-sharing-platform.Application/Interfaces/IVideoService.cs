using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using backend_video_sharing_platform.Application.DTOs;
using backend_video_sharing_platform.Domain.Entities;

namespace backend_video_sharing_platform.Application.Interfaces
{
    public interface IVideoService
    {
        Task<PresignUrlResponse> GenerateUploadUrlAsync(PresignUrlRequest request, string userId);
        Task<List<VideoResponseDto>> GetAllVideosAsync();
        Task<List<VideoResponseDto>> GetVideosByChannelIdAsync(string channelId);

        Task UploadThumbnailAsync(
    string videoId,
    string userId,
    Stream fileStream,
    string fileName,
    string contentType,
    CancellationToken ct = default);

        Task<Video?> GetVideoByIdAsync(string videoId);

    }
}
