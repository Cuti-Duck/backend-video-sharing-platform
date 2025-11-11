using System.Security.Claims;
using Amazon.DynamoDBv2.DataModel;
using backend_video_sharing_platform.API.Models.Requests;
using backend_video_sharing_platform.Application.DTOs.User;
using backend_video_sharing_platform.Application.Interfaces;
using backend_video_sharing_platform.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace backend_video_sharing_platform.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UserController : ControllerBase
    {
        private readonly IDynamoDBContext _db;
        private readonly ILogger<UserController> _logger;
        private readonly IUserService _userService;
        public UserController(IDynamoDBContext db, ILogger<UserController> logger, IUserService userService)
        {
            _db = db;
            _logger = logger;
            _userService = userService;
        }

        [Authorize]
        [HttpGet("me")]
        public async Task<IActionResult> GetCurrentUserAsync()
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)
                          ?? User.FindFirstValue("sub");

                if (string.IsNullOrEmpty(userId))
                {
                    _logger.LogWarning("UserId not found in token");
                    return Unauthorized(new { message = "Không tìm thấy userId trong token." });
                }

                _logger.LogInformation($"Fetching user info for userId: {userId}");

                var user = await _db.LoadAsync<User>(userId);

                if (user == null)
                {
                    _logger.LogWarning($"User not found: {userId}");
                    return NotFound(new { message = "User không tồn tại." });
                }

                var response = new UserResponse
                {
                    UserId = user.UserId,
                    Email = user.Email,
                    Name = user.Name,
                    Gender = user.Gender,
                    BirthDate = user.BirthDate,
                    PhoneNumber = user.PhoneNumber,
                    AvatarUrl = user.AvatarUrl,
                    ChannelId = user.ChannelId,
                    CreatedAt = user.CreatedAt
                };

                return Ok(new
                {
                    message = "Lấy thông tin user thành công.",
                    data = response
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting user info");
                return StatusCode(500, new
                {
                    message = "Lỗi server",
                    error = ex.Message
                });
            }
        }

        [Authorize]
        [HttpPut("update-profile")]
        public async Task<IActionResult> UpdateProfile([FromBody] UpdateUserRequest request)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)
                      ?? User.FindFirstValue("sub");

            if (string.IsNullOrEmpty(userId))
                return Unauthorized(new { message = "Không tìm thấy userId trong token." });

            var updated = await _userService.UpdateUserAsync(userId, request);

            if (!updated)
                return NotFound(new { message = "User không tồn tại hoặc không có gì để cập nhật." });

            return Ok(new { message = "Cập nhật thông tin user + đồng bộ Channel thành công." });
        }

        [Authorize]
        [HttpPost("avatar")]
        [Consumes("multipart/form-data")] // ✅ fix swagger
        [RequestSizeLimit(2 * 1024 * 1024)] // 2MB
        public async Task<IActionResult> UploadAvatar([FromForm] UploadAvatarRequest request, CancellationToken ct)
        {
            if (request.File == null || request.File.Length == 0)
                return BadRequest(new { message = "Vui lòng chọn file ảnh." });

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)
                       ?? User.FindFirstValue("sub");

            if (string.IsNullOrEmpty(userId))
                return Unauthorized(new { message = "Không tìm thấy userId trong token." });

            try
            {
                await using var stream = request.File.OpenReadStream();
                var result = await _userService.UploadAvatarAsync(userId, stream, request.File.FileName, request.File.ContentType, ct);

                if (result == null)
                    return NotFound(new { message = "User không tồn tại." });

                return Ok(new { message = "Upload avatar thành công.", avatarUrl = result.AvatarUrl });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Lỗi server khi upload avatar.", error = ex.Message });
            }
        }
    }
}
