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

        public VideoController(IVideoService videoService, IMapper mapper)
        {
            _videoService = videoService;
            _mapper = mapper;
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

        [Authorize]
        [HttpPost("{videoId}/thumbnail")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> UploadThumbnail(
    string videoId,
    [FromForm] UploadThumbnailRequest request,
    CancellationToken ct)
        {
            if (request.Thumbnail == null || request.Thumbnail.Length == 0)
                throw new BadRequestException("Vui lòng chọn file thumbnail.");

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)
                       ?? User.FindFirstValue("sub");

            if (string.IsNullOrEmpty(userId))
                throw new UnauthorizedAccessException("Không tìm thấy userId trong token.");

            await using var stream = request.Thumbnail.OpenReadStream();

            await _videoService.UploadThumbnailAsync(
                videoId,
                userId,
                stream,
                request.Thumbnail.FileName,
                request.Thumbnail.ContentType,
                ct
            );

            return Ok(new { message = "Upload thumbnail thành công." });
        }

        [HttpGet("{videoId}")]
        public async Task<IActionResult> GetVideoById(string videoId)
        {
            var video = await _videoService.GetVideoByIdAsync(videoId);

            if (video == null)
                return NotFound(new { message = "Video không tồn tại." });

            var response = _mapper.Map<VideoResponse>(video);

            return Ok(response);
        }

    }
}
