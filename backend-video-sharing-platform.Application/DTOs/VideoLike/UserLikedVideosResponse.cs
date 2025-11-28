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
        public string ChannelId { get; set; } = string.Empty;
        public string UserId { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string PlaybackUrl { get; set; } = string.Empty;
        public string Key { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public double Duration { get; set; }
        public string ThumbnailUrl { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public long ViewCount { get; set; }
        public long LikeCount { get; set; }
        public int CommentCount { get; set; }
        public string CreatedAt { get; set; } = string.Empty;
        public string UpdatedAt { get; set; } = string.Empty;

        // Additional fields
        public string LikedAt { get; set; } = string.Empty;
        public string ChannelName { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public string UserAvatarUrl { get; set; } = string.Empty;
    }
}