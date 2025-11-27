using backend_video_sharing_platform.Application.DTOs.VideoLike;
using backend_video_sharing_platform.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace backend_video_sharing_platform.API.Controllers
{
    [ApiController]
    [Route("api/videos")]
    public class VideoLikesController : ControllerBase
    {
        private readonly IVideoLikeService _service;
        private readonly ILogger<VideoLikesController> _logger;

        public VideoLikesController(
            IVideoLikeService service,
            ILogger<VideoLikesController> logger)
        {
            _service = service;
            _logger = logger;
        }

        /// <summary>
        /// Toggle like/unlike for a video (Smart endpoint - one API for both actions)
        /// </summary>
        [HttpPost("{videoId}/like")]
        [Authorize]
        public async Task<IActionResult> ToggleLike(string videoId)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                         ?? User.FindFirst("sub")?.Value;

            if (string.IsNullOrEmpty(userId))
                return Unauthorized("User ID not found in token");

            var result = await _service.ToggleLikeAsync(userId, videoId);
            return Ok(result);
        }

        /// <summary>
        /// Get like status for a specific video (for current user)
        /// </summary>
        [HttpGet("{videoId}/like/status")]
        [Authorize]
        public async Task<IActionResult> GetLikeStatus(string videoId)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                         ?? User.FindFirst("sub")?.Value;

            if (string.IsNullOrEmpty(userId))
                return Unauthorized("User ID not found in token");

            var result = await _service.GetLikeStatusAsync(userId, videoId);
            return Ok(result);
        }

        /// <summary>
        /// Get all videos liked by current user
        /// </summary>
        [HttpGet("liked")]
        [Authorize]
        public async Task<IActionResult> GetMyLikedVideos()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                         ?? User.FindFirst("sub")?.Value;

            if (string.IsNullOrEmpty(userId))
                return Unauthorized("User ID not found in token");

            var result = await _service.GetUserLikedVideosAsync(userId);
            return Ok(result);
        }
    }
}