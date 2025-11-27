using backend_video_sharing_platform.Application.DTOs.Search;
using backend_video_sharing_platform.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace backend_video_sharing_platform.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SearchController : ControllerBase
    {
        private readonly ISearchService _searchService;
        private readonly ILogger<SearchController> _logger;

        public SearchController(
            ISearchService searchService,
            ILogger<SearchController> logger)
        {
            _searchService = searchService;
            _logger = logger;
        }

        /// <summary>
        /// Search for videos and channels
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> Search(
            [FromQuery] string q,
            [FromQuery] string? type = null,
            [FromQuery] string sortBy = "relevance",
            [FromQuery] int limit = 20,
            [FromQuery] int offset = 0)
        {
            if (string.IsNullOrWhiteSpace(q))
                return BadRequest("Search query is required");

            // Validate limit
            if (limit < 1) limit = 20;
            if (limit > 100) limit = 100;

            // Validate offset
            if (offset < 0) offset = 0;

            // Validate type
            if (!string.IsNullOrEmpty(type))
            {
                var validTypes = new[] { "video", "channel" };
                if (!validTypes.Contains(type.ToLower()))
                    return BadRequest("Invalid type. Must be 'video' or 'channel'");
            }

            var request = new SearchRequest
            {
                Query = q,
                Type = type,
                SortBy = sortBy,
                Limit = limit,
                Offset = offset
            };

            var result = await _searchService.SearchAsync(request);
            return Ok(result);
        }
    }
}