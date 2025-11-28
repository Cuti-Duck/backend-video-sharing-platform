using backend_video_sharing_platform.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace backend_video_sharing_platform.API.Controllers
{
    [ApiController]
    [Route("api/videos")]
    public class VideoViewsController : ControllerBase
    {
        private readonly IVideoService _service;

        public VideoViewsController(IVideoService service)
        {
            _service = service;
        }

        [HttpPost("{videoId}/views")]
        public async Task<IActionResult> IncreaseView(string videoId)
        {
            var result = await _service.IncreaseViewCountAsync(videoId);
            return Ok(result);
        }

        [HttpGet("{videoId}/views")]
        public async Task<IActionResult> GetViewCount(string videoId)
        {
            var result = await _service.GetViewCountAsync(videoId);
            return Ok(result);
        }
    }
}
