using System.Threading.Tasks;
using backend_video_sharing_platform.Application.DTOs.Livestream;

// ⚠️ Sử dụng ALIAS để tránh conflict với Amazon.IVS.Model.StreamSession
using DbStreamSession = backend_video_sharing_platform.Domain.Entities.StreamSession;

namespace backend_video_sharing_platform.Application.Interfaces
{
    public interface IIVSService
    {
        Task<CreateLivestreamResponse> CreateLivestreamAsync(string userId);

        Task<DbStreamSession> UpdateLivestreamMetadataAsync(string userId, UpdateLivestreamMetadataRequest request);

        Task<DbStreamSession?> GetPendingStreamSessionAsync(string userId);
    }
}