using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace backend_video_sharing_platform.Application.DTOs.VideoLike
{
    public class ToggleLikeRequest
    {
        public string VideoId { get; set; } = string.Empty;
    }
}
