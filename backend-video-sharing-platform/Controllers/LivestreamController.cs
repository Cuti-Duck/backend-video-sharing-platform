using System.Security.Claims;
using backend_video_sharing_platform.Application.DTOs.Livestream;
using backend_video_sharing_platform.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

// ⚠️ Sử dụng ALIAS để tránh conflict
using DbStreamSession = backend_video_sharing_platform.Domain.Entities.StreamSession;

namespace backend_video_sharing_platform.API.Controllers
{
    [ApiController]
    [Route("api/livestreams")]
    public class LivestreamController : ControllerBase
    {
        private readonly IIVSService _ivsService;
        private readonly ILogger<LivestreamController> _logger;

        public LivestreamController(
            IIVSService ivsService,
            ILogger<LivestreamController> logger)
        {
            _ivsService = ivsService;
            _logger = logger;
        }

        /// <summary>
        /// Tạo hoặc lấy IVS channel cho user (1 user = 1 channel)
        /// </summary>
        [Authorize]
        [HttpPost("create")]
        public async Task<IActionResult> CreateLivestream()
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)
                          ?? User.FindFirstValue("sub");

                if (string.IsNullOrEmpty(userId))
                {
                    _logger.LogWarning("UserId not found in token claims");
                    var allClaims = User.Claims.Select(c => new { c.Type, c.Value }).ToList();
                    _logger.LogWarning($"Available claims: {string.Join(", ", allClaims.Select(c => c.Type))}");
                    return Unauthorized(new
                    {
                        message = "Cannot extract userId from token claims.",
                        availableClaims = allClaims
                    });
                }

                _logger.LogInformation($"Creating/getting livestream for userId: {userId}");
                var result = await _ivsService.CreateLivestreamAsync(userId);

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating livestream");
                return StatusCode(500, new
                {
                    message = "An error occurred while creating the livestream.",
                    error = ex.Message
                });
            }
        }

        /// <summary>
        /// Cập nhật metadata cho livestream
        /// 
        /// Hỗ trợ 2 cách:
        /// 1. JSON (application/json): Chỉ title, description, thumbnailUrl
        /// 2. Form-data (multipart/form-data): Title, description + upload thumbnail file
        /// </summary>
        [Authorize]
        [HttpPost("metadata")]
        [RequestSizeLimit(10 * 1024 * 1024)] // 10MB limit (cho cả request)
        public async Task<IActionResult> UpdateLivestreamMetadata([FromForm] UpdateLivestreamMetadataRequest request)
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)
                          ?? User.FindFirstValue("sub");

                if (string.IsNullOrEmpty(userId))
                    return Unauthorized(new { message = "UserId not found in token" });

                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                _logger.LogInformation($"Updating livestream metadata for userId: {userId}");

                // Log để debug
                if (request.Thumbnail != null)
                {
                    _logger.LogInformation($"Thumbnail uploaded: {request.Thumbnail.FileName}, size: {request.Thumbnail.Length} bytes");
                }

                var streamSession = await _ivsService.UpdateLivestreamMetadataAsync(userId, request);

                return Ok(new
                {
                    message = "Livestream metadata updated successfully",
                    streamSession = new
                    {
                        streamSession.StreamId,
                        streamSession.Title,
                        streamSession.Description,
                        streamSession.ThumbnailUrl,
                        streamSession.Status,
                        streamSession.CreatedAt,
                        streamSession.UpdatedAt
                    }
                });
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Invalid metadata input");
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating livestream metadata");
                return StatusCode(500, new
                {
                    message = "An error occurred while updating metadata.",
                    error = ex.Message
                });
            }
        }

        /// <summary>
        /// Lấy thông tin StreamSession đang pending (chưa bắt đầu)
        /// </summary>
        [Authorize]
        [HttpGet("pending-session")]
        public async Task<IActionResult> GetPendingStreamSession()
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)
                          ?? User.FindFirstValue("sub");

                if (string.IsNullOrEmpty(userId))
                    return Unauthorized(new { message = "UserId not found in token" });

                var streamSession = await _ivsService.GetPendingStreamSessionAsync(userId);

                if (streamSession == null)
                {
                    return Ok(new
                    {
                        message = "No pending stream session found",
                        streamSession = (object?)null
                    });
                }

                return Ok(new
                {
                    message = "Pending stream session found",
                    streamSession = new
                    {
                        streamSession.StreamId,
                        streamSession.Title,
                        streamSession.Description,
                        streamSession.ThumbnailUrl,
                        streamSession.Status,
                        streamSession.CreatedAt,
                        streamSession.UpdatedAt
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting pending stream session");
                return StatusCode(500, new
                {
                    message = "An error occurred while getting pending stream session.",
                    error = ex.Message
                });
            }
        }

        /// <summary>
        /// Lấy thông tin channel của user hiện tại
        /// </summary>
        [Authorize]
        [HttpGet("my-channel")]
        public async Task<IActionResult> GetMyChannel()
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)
                          ?? User.FindFirstValue("sub");

                if (string.IsNullOrEmpty(userId))
                    return Unauthorized(new { message = "UserId not found" });

                return Ok(new { userId, message = "Success" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting channel");
                return StatusCode(500, new { error = ex.Message });
            }
        }
    }
}