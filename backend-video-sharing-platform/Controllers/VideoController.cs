using System.Security.Claims;
using AutoMapper;
using backend_video_sharing_platform.Application.Common.Exceptions;
using backend_video_sharing_platform.Application.DTOs;
using backend_video_sharing_platform.Application.DTOs.Video;
using backend_video_sharing_platform.Application.Interfaces;
using backend_video_sharing_platform.Models.Requests;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace backend_video_sharing_platform.Api.Controllers
{
    [ApiController]
    [Route("api/videos")]
    public class VideoController : ControllerBase
    {
        private readonly IVideoService _videoService;
        private readonly IMapper _mapper;
        private readonly ILogger<VideoController> _logger;


        public VideoController(IVideoService videoService, IMapper mapper, ILogger<VideoController> logger)
        {
            _videoService = videoService;
            _mapper = mapper;
            _logger = logger;
        }

        [HttpPost("create")]
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
        [HttpGet("all")]
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

        [Authorize]
        [HttpPost("{videoId}/thumbnail")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> UploadThumbnail(
    string videoId,
    [FromForm] UploadThumbnailRequest request,
    CancellationToken ct)
        {
            if (request.Thumbnail == null || request.Thumbnail.Length == 0)
                throw new BadRequestException("Please select a thumbnail file.");

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)
                       ?? User.FindFirstValue("sub");

            if (string.IsNullOrEmpty(userId))
                throw new UnauthorizedAccessException("userId not found in token.");

            await using var stream = request.Thumbnail.OpenReadStream();

            await _videoService.UploadThumbnailAsync(
                videoId,
                userId,
                stream,
                request.Thumbnail.FileName,
                request.Thumbnail.ContentType,
                ct
            );

            return Ok(new { message = "Thumbnail uploaded successfully." });
        }

        [HttpGet("{videoId}")]
        public async Task<IActionResult> GetVideoById(string videoId)
        {
            var video = await _videoService.GetVideoByIdAsync(videoId);

            if (video == null)
                return NotFound(new { message = "Video does not exist." });

            var response = _mapper.Map<VideoResponse>(video);

            return Ok(response);
        }
        [Authorize]
        [HttpDelete("{videoId}")]
        public async Task<IActionResult> DeleteVideo(string videoId)
        {
            // Lấy userId từ JWT token
            var currentUserId = User.FindFirst("userId")?.Value
                             ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                             ?? User.FindFirst("sub")?.Value; // thử cả claim "sub"

            _logger.LogInformation(
                "DELETE request for video {VideoId} from user {UserId}",
                videoId, currentUserId);

            if (string.IsNullOrEmpty(currentUserId))
            {
                _logger.LogWarning("Unauthorized delete attempt - no userId in token");
                return Unauthorized(new { message = "User information not found" });
            }

            await _videoService.DeleteVideoAsync(videoId, currentUserId);
            return NoContent();
        }

        [Authorize]
        [HttpPut("{videoId}")]
        public async Task<IActionResult> UpdateVideo(string videoId, [FromBody] UpdateVideoRequest request)
        {
            // Lấy userId từ JWT (ưu tiên access token)
            var currentUserId =
                User.FindFirst("userId")?.Value
                ?? User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value
                ?? User.FindFirst("sub")?.Value;

            if (string.IsNullOrEmpty(currentUserId))
            {
                _logger.LogWarning("Unauthorized update attempt - no userId in token");
                return Unauthorized(new { message = "userId not found in token." });
            }

            _logger.LogInformation(
                "UPDATE request for video {VideoId} from user {UserId}",
                videoId, currentUserId);

            var updatedVideo = await _videoService.UpdateVideoAsync(videoId, currentUserId, request);

            return Ok(new
            {
                message = "Video updated successfully.",
                data = updatedVideo
            });
        }

        [HttpGet("trending")]
        public async Task<IActionResult> GetTrending([FromQuery] int limit = 20)
        {
            var result = await _videoService.GetTrendingAsync(limit);
            return Ok(result);
        }
    }
}
