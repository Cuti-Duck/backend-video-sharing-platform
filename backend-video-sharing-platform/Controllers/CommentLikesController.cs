using backend_video_sharing_platform.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace backend_video_sharing_platform.API.Controllers
{
    [ApiController]
    [Route("api/comments")]
    public class CommentLikesController : ControllerBase
    {
        private readonly ICommentLikeService _service;

        public CommentLikesController(ICommentLikeService service)
        {
            _service = service;
        }
        [HttpPost("/api/videos/{videoId}/comments/{commentId}/like")]
        [Authorize]
        public async Task<IActionResult> ToggleLike(string videoId, string commentId)
        {
            try
            {
                // Fix: Thử nhiều cách lấy userId
                var userId = User.FindFirst("sub")?.Value
                          ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                          ?? User.FindFirst("user_id")?.Value
                          ?? User.FindFirst("cognito:username")?.Value;

                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(new { message = "User ID not found in token" });
                }

                if (string.IsNullOrEmpty(videoId) || string.IsNullOrEmpty(commentId))
                {
                    return BadRequest(new { message = "VideoId and CommentId are required" });
                }

                var result = await _service.ToggleLikeAsync(videoId, commentId, userId);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred", error = ex.Message });
            }
        }
        [HttpGet("{commentId}/likes/count")]
        public async Task<IActionResult> GetLikeCount(string commentId)
        {
            var result = await _service.GetLikeCountAsync(commentId);
            return Ok(result);
        }

        [HttpGet("/api/videos/{videoId}/comments/{commentId}/like/status")]
        [Authorize]
        public async Task<IActionResult> CheckLikeStatus(string videoId, string commentId)
        {
            var userId = User.FindFirst("sub")?.Value
                      ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                      ?? User.FindFirst("user_id")?.Value
                      ?? User.FindFirst("cognito:username")?.Value;

            if (string.IsNullOrEmpty(userId))
                return Unauthorized(new { message = "User ID not found in token" });

            var result = await _service.CheckLikedAsync(commentId, userId);
            return Ok(result);
        }

    }
}

