using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using backend_video_sharing_platform.Application.Interfaces;

namespace backend_video_sharing_platform.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class LivestreamController : ControllerBase
    {
        private readonly IIVSService _ivs;

        public LivestreamController(IIVSService ivs)
        {
            _ivs = ivs;
        }

        [Authorize]
        [HttpPost("create")]
        public async Task<IActionResult> CreateChannel()
        {
            // Lấy tất cả claims để debug
            var allClaims = User.Claims.Select(c => $"{c.Type}: {c.Value}").ToList();
            Console.WriteLine("==== TOKEN CLAIMS ====");
            foreach (var claim in allClaims)
                Console.WriteLine(claim);

            // Lấy userId đúng cách
            var userId = User.FindFirst("sub")?.Value
                       ?? User.FindFirst("username")?.Value
                       ?? User.FindFirst("cognito:username")?.Value
                       ?? User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userId))
                return Unauthorized("Cannot extract userId from token claims.");

            var result = await _ivs.CreateChannelAsync(userId);
            return Ok(result);
        }

        [Authorize]
        [HttpGet("videos")]
        public async Task<IActionResult> GetVideos()
        {
            //  Lấy userId trực tiếp từ AccessToken
            var userId = User.FindFirst("sub")?.Value
                       ?? User.FindFirst("username")?.Value
                       ?? User.FindFirst("cognito:username")?.Value
                       ?? User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userId))
                return Unauthorized("Cannot extract userId from token claims.");

            var result = await _ivs.GetVideosByUserIdAsync(userId);
            return Ok(result);
        }
    }
}
