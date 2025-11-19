using backend_video_sharing_platform.Application.DTOs;
using backend_video_sharing_platform.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace backend_video_sharing_platform.Api.Controllers
{
    [ApiController]
    [Route("api/videos")]
    public class VideoController : ControllerBase
    {
        private readonly IVideoService _videoService;

        public VideoController(IVideoService videoService)
        {
            _videoService = videoService;
        }

        [HttpPost("presign")]
        [Authorize]
        public async Task<IActionResult> GeneratePresignUrl([FromBody] PresignUrlRequest request)
        {
            // Lấy userId từ access token (Cognito claim "sub")
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                         ?? User.FindFirst("sub")?.Value;

            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized(new { message = "Cannot resolve user id from token" });
            }

            var result = await _videoService.GenerateUploadUrlAsync(request, userId);

            return Ok(result);
        }
        [HttpGet]
        public async Task<IActionResult> GetAllVideos()
        {
            var videos = await _videoService.GetAllVideosAsync();
            return Ok(videos);
        }

        [HttpGet("channel/{channelId}")]
        public async Task<IActionResult> GetVideosByChannel(string channelId)
        {
            var videos = await _videoService.GetVideosByChannelIdAsync(channelId);
            return Ok(videos);
        }
    }
}
