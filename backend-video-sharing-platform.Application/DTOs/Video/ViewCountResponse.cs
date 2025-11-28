namespace backend_video_sharing_platform.Application.DTOs.Video
{
    public class ViewCountResponse
    {
        public string VideoId { get; set; } = string.Empty;
        public int ViewCount { get; set; }
    }
}
