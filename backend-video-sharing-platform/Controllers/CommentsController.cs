using backend_video_sharing_platform.Application.DTOs.Comment;
using backend_video_sharing_platform.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace backend_video_sharing_platform.API.Controllers
{
    [ApiController]
    [Route("api")]
    public class CommentsController : ControllerBase
    {
        private readonly ICommentService _commentService;
        private readonly ILogger<CommentsController> _logger;

        public CommentsController(
            ICommentService commentService,
            ILogger<CommentsController> logger)
        {
            _commentService = commentService;
            _logger = logger;
        }

        /// <summary>
        /// Get all comments for a video
        /// </summary>
        [HttpGet("videos/{videoId}/comments")]
        public async Task<IActionResult> GetComments(
            string videoId,
            [FromQuery] string sortBy = "recent",
            [FromQuery] int limit = 20,
            [FromQuery] int offset = 0,
            [FromQuery] bool includeReplies = true,
            [FromQuery] string? parentCommentId = null)
        {
            if (limit < 1) limit = 20;
            if (limit > 100) limit = 100;
            if (offset < 0) offset = 0;

            var request = new GetCommentsRequest
            {
                SortBy = sortBy,
                Limit = limit,
                Offset = offset,
                IncludeReplies = includeReplies,
                ParentCommentId = parentCommentId
            };

            var result = await _commentService.GetCommentsAsync(videoId, request);
            return Ok(result);
        }

        /// <summary>
        /// Create a new comment or reply
        /// </summary>
        [HttpPost("videos/{videoId}/comments")]
        [Authorize]
        public async Task<IActionResult> CreateComment(
            string videoId,
            [FromBody] CreateCommentRequest request)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                         ?? User.FindFirst("sub")?.Value;

            if (string.IsNullOrEmpty(userId))
                return Unauthorized("User ID not found in token");

            var result = await _commentService.CreateCommentAsync(videoId, userId, request);
            return CreatedAtAction(
                nameof(CreateComment),
                new { videoId, commentId = result.CommentId },
                result
            );
        }

        /// <summary>
        /// Update/Edit a comment
        /// </summary>
        [HttpPut("videos/{videoId}/comments/{commentId}")]
        [Authorize]
        public async Task<IActionResult> UpdateComment(
            string videoId,
            string commentId,
            [FromBody] UpdateCommentRequest request)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                         ?? User.FindFirst("sub")?.Value;

            if (string.IsNullOrEmpty(userId))
                return Unauthorized("User ID not found in token");

            var result = await _commentService.UpdateCommentAsync(videoId, commentId, userId, request);
            return Ok(result);
        }

        /// <summary>
        /// Delete a comment (hard delete with cascade)
        /// Deletes the comment and ALL its replies recursively
        /// Only comment owner or video owner can delete
        /// </summary>
        [HttpDelete("videos/{videoId}/comments/{commentId}")]
        [Authorize]
        public async Task<IActionResult> DeleteComment(
            string videoId,
            string commentId)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                         ?? User.FindFirst("sub")?.Value;

            if (string.IsNullOrEmpty(userId))
                return Unauthorized("User ID not found in token");

            var result = await _commentService.DeleteCommentAsync(videoId, commentId, userId);
            return Ok(result);
        }
    }
}