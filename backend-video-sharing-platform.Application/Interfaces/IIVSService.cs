using System.Threading.Tasks;
using backend_video_sharing_platform.Application.DTOs.Livestream;

namespace backend_video_sharing_platform.Application.Interfaces
{
    public interface IIVSService
    {
        Task<CreateLivestreamResponse> CreateLivestreamAsync(string userId);
    }
}
