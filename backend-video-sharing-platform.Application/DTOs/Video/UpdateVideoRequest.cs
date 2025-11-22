using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace backend_video_sharing_platform.Application.DTOs.Video
{
    public class UpdateVideoRequest
    {
        public string? Title { get; set; }
        public string? Description { get; set; }
    }
}
