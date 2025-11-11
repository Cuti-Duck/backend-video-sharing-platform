using System.Security.Claims;
using backend_video_sharing_platform.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace backend_video_sharing_platform.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
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
                // QUAN TRỌNG: Lấy userId đúng cách
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)
                          ?? User.FindFirstValue("sub");

                if (string.IsNullOrEmpty(userId))
                {
                    _logger.LogWarning("UserId not found in token claims");

                    // Debug: In ra tất cả claims
                    var allClaims = User.Claims.Select(c => new { c.Type, c.Value }).ToList();
                    _logger.LogWarning($"Available claims: {string.Join(", ", allClaims.Select(c => c.Type))}");

                    return Unauthorized(new
                    {
                        message = "Cannot extract userId from token claims.",
                        availableClaims = allClaims // Debug only, xóa khi production
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
                    message = "Lỗi khi tạo livestream",
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

                // Lấy thông tin channel từ DynamoDB
                // (Bạn có thể gọi ChannelService.GetChannelByIdAsync)

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