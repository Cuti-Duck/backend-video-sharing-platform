namespace backend_video_sharing_platform.Models.Requests
{
    public class UploadThumbnailRequest
    {
        public IFormFile Thumbnail { get; set; } = null!;
    }
}
