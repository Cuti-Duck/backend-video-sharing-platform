namespace backend_video_sharing_platform.Application.DTOs.Video
{
    public class LikeVideoResponse
    {
        public string VideoId { get; set; } = string.Empty;
        public bool Liked { get; set; }
        public int LikeCount { get; set; }
    }
}
