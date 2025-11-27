using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace backend_video_sharing_platform.Application.DTOs.Comment
{
    public class GetCommentsResponse
    {
        public string VideoId { get; set; } = string.Empty;
        public List<CommentDto> Comments { get; set; } = new();
        public int TotalCount { get; set; }
        public int RootCommentsCount { get; set; }
        public int RepliesCount { get; set; }
    }

    public class CommentDto
    {
        public string CommentId { get; set; } = string.Empty;
        public string VideoId { get; set; } = string.Empty;
        public string UserId { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public string? UserAvatarUrl { get; set; }
        public string Content { get; set; } = string.Empty;
        public string? ParentCommentId { get; set; }
        public int LikeCount { get; set; }
        public int ReplyCount { get; set; }
        public bool IsEdited { get; set; }
        public string CreatedAt { get; set; } = string.Empty;
        public string? UpdatedAt { get; set; }

        // Nested replies (optional - chỉ load khi cần)
        public List<CommentDto>? Replies { get; set; }
    }
}