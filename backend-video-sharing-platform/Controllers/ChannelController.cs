using System.Security.Claims;
using backend_video_sharing_platform.Application.DTOs.Channel;
using backend_video_sharing_platform.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace backend_video_sharing_platform.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ChannelController : ControllerBase
    {
        private readonly IChannelService _channelService;

        public ChannelController(IChannelService channelService)
        {
            _channelService = channelService;
        }

        //PUBLIC - không cần Authorize
        [HttpGet("{channelId}")]
        public async Task<IActionResult> GetChannelById(string channelId, CancellationToken ct)
        {
            var channel = await _channelService.GetChannelByIdAsync(channelId, ct);
            if (channel == null)
                return NotFound(new { message = "Không tìm thấy kênh." });

            return Ok(new
            {
                message = "Lấy thông tin kênh thành công.",
                data = channel
            });
        }

        // PRIVATE - chỉ chủ kênh mới được sửa mô tả
        [Authorize]
        [HttpPut("update-description")]
        public async Task<IActionResult> UpdateDescription([FromBody] UpdateDescriptionRequest request, CancellationToken ct)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)
                      ?? User.FindFirstValue("sub");

            if (string.IsNullOrEmpty(userId))
                return Unauthorized(new { message = "Không xác định được người dùng từ token." });

            var success = await _channelService.UpdateDescriptionAsync(userId, request.Description, ct);

            if (!success)
                return NotFound(new { message = "Không tìm thấy kênh để cập nhật." });

            return Ok(new { message = "Cập nhật mô tả kênh thành công." });
        }
    }
}
