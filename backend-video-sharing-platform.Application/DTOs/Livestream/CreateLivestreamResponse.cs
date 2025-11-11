namespace backend_video_sharing_platform.Application.DTOs.Livestream
{
    public class CreateLivestreamResponse
    {
        public string Message { get; set; } = string.Empty;
        public string ChannelArn { get; set; } = string.Empty;
        public string PlaybackUrl { get; set; } = string.Empty;
        public string IngestServer { get; set; } = string.Empty;
        public string StreamKeyArn { get; set; } = string.Empty;
        public string StreamKey { get; set; } = string.Empty;
    }
}
