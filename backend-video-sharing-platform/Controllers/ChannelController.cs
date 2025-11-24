using System.Security.Claims;
using backend_video_sharing_platform.Application.DTOs.Channel;
using backend_video_sharing_platform.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace backend_video_sharing_platform.API.Controllers
{
    [ApiController]
    [Route("api/channels")]
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
                return NotFound(new { message = "Channel not found." });

            return Ok(new
            {
                message = "Channel information retrieved successfully.",
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
                return Unauthorized(new { message = "Unable to identify the user from the token." });

            var success = await _channelService.UpdateDescriptionAsync(userId, request.Description, ct);

            if (!success)
                return NotFound(new { message = "No channel found to update." });

            return Ok(new { message = "Channel description updated successfully." });
        }
    }
}
