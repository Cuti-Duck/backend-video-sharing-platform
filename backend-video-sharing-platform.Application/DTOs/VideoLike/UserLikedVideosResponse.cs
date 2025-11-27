using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace backend_video_sharing_platform.Application.DTOs.VideoLike
{
    public class UserLikedVideosResponse
    {
        public List<LikedVideoDto> Videos { get; set; } = new();
        public int TotalCount { get; set; }
    }

    public class LikedVideoDto
    {
        public string VideoId { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string? ThumbnailUrl { get; set; }
        public string LikedAt { get; set; } = string.Empty;
        public int LikeCount { get; set; }
        public int ViewCount { get; set; }
        public string ChannelName { get; set; } = string.Empty;
    }
}
