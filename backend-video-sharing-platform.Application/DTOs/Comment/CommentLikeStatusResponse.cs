using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace backend_video_sharing_platform.Application.DTOs.Comment
{
    public class CommentLikeStatusResponse
    {
        public string CommentId { get; set; } = string.Empty;
        public bool Liked { get; set; }
    }
}
