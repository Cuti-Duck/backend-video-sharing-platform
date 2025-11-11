using Microsoft.AspNetCore.Http;

namespace backend_video_sharing_platform.API.Models.Requests
{
    public class UploadAvatarRequest
    {
        public IFormFile File { get; set; } = default!;
    }
}
