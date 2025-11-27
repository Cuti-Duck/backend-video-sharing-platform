using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace backend_video_sharing_platform.Application.DTOs.VideoLike
{
    public class VideoLikeStatusResponse
    {
        public string VideoId { get; set; } = string.Empty;
        public bool IsLiked { get; set; }
        public int TotalLikes { get; set; }
    }
}
