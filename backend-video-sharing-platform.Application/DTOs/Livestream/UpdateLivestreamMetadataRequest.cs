using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace backend_video_sharing_platform.Application.DTOs.Livestream
{
    /// <summary>
    /// Request cho API POST /metadata
    /// CHỈ HỖ TRỢ upload file thumbnail (multipart/form-data)
    /// KHÔNG còn field ThumbnailUrl
    /// </summary>
    public class UpdateLivestreamMetadataRequest
    {
        [Required(ErrorMessage = "Title is required")]
        [StringLength(200, MinimumLength = 1, ErrorMessage = "Title must be between 1 and 200 characters")]
        public string Title { get; set; } = string.Empty;

        [StringLength(2000, ErrorMessage = "Description cannot exceed 2000 characters")]
        public string? Description { get; set; }

        /// <summary>
        /// Thumbnail file upload
        /// Optional - nếu không upload sẽ giữ thumbnail cũ (nếu có)
        /// </summary>
        public IFormFile? Thumbnail { get; set; }
    }
}