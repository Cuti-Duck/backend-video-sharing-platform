using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace backend_video_sharing_platform.Application.DTOs
{
    public class PresignUrlResponse
    {
        public string VideoId { get; set; } = default!;
        public string UploadUrl { get; set; } = default!;
    }
}
