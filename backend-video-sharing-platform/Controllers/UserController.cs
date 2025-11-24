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
    [Route("api/users")]
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
                    return Unauthorized(new { message = "userId not found in token." });
                }

                _logger.LogInformation($"Fetching user info for userId: {userId}");

                var user = await _db.LoadAsync<User>(userId);

                if (user == null)
                {
                    _logger.LogWarning($"User not found: {userId}");
                    return NotFound(new { message = "User does not exist." });
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
                    message = "User information retrieved successfully.",
                    data = response
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting user info");
                return StatusCode(500, new
                {
                    message = "Server error",
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
                return Unauthorized(new { message = "userId not found in token." });

            var updated = await _userService.UpdateUserAsync(userId, request);

            if (!updated)
                return NotFound(new { message = "User does not exist or there is nothing to update." });

            return Ok(new { message = "User information updated and channel synchronized successfully." });
        }

        [Authorize]
        [HttpPost("avatar")]
        [Consumes("multipart/form-data")] // ✅ fix swagger
        [RequestSizeLimit(2 * 1024 * 1024)] // 2MB
        public async Task<IActionResult> UploadAvatar([FromForm] UploadAvatarRequest request, CancellationToken ct)
        {
            if (request.File == null || request.File.Length == 0)
                return BadRequest(new { message = "Please select an image file." });

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)
                       ?? User.FindFirstValue("sub");

            if (string.IsNullOrEmpty(userId))
                return Unauthorized(new { message = "userId not found in token." });

            try
            {
                await using var stream = request.File.OpenReadStream();
                var result = await _userService.UploadAvatarAsync(userId, stream, request.File.FileName, request.File.ContentType, ct);

                if (result == null)
                    return NotFound(new { message = "User does not exist." });

                return Ok(new { message = "Avatar uploaded successfully.", avatarUrl = result.AvatarUrl });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Server error while uploading avatar.", error = ex.Message });
            }
        }

        [HttpGet("all")]
        public async Task<IActionResult> GetAllUsers()
        {
            var users = await _userService.GetAllUsersAsync();

            return Ok(new
            {
                message = "User list retrieved successfully.",
                count = users.Count(),
                data = users
            });
        }

        [HttpGet("{userId}")]
        public async Task<IActionResult> GetUserById(string userId)
        {
            var user = await _userService.GetUserByIdAsync(userId);

            return Ok(new
            {
                message = "User information retrieved successfully.",
                data = user
            });
        }
    }
}
