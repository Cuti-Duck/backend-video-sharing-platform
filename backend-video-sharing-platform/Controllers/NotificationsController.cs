using System.Security.Claims;
using backend_video_sharing_platform.Application.Common.Exceptions;
using backend_video_sharing_platform.Application.Interfaces;
using backend_video_sharing_platform.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/notifications")]
[Authorize]
public class NotificationsController : ControllerBase
{
    private readonly INotificationService _notificationService;
    private readonly ILogger<NotificationsController> _logger;

    public NotificationsController(
        INotificationService notificationService,
        ILogger<NotificationsController> logger)
    {
        _notificationService = notificationService;
        _logger = logger;
    }

    [HttpGet]
    public async Task<IActionResult> GetMyNotifications(
        [FromQuery] bool unreadOnly = false,
        [FromQuery] int limit = 20,
        [FromQuery] string? cursor = null)
    {
        try
        {
            var userId = GetUserId();
            if (string.IsNullOrEmpty(userId))
                return Unauthorized(new { message = "User ID not found in token" });

            var result = await _notificationService.GetUserNotificationsAsync(
                userId,
                unreadOnly,
                limit,
                cursor
            );

            return Ok(new
            {
                message = "Notifications retrieved successfully",
                data = result
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting notifications");
            return StatusCode(500, new
            {
                message = "Server error while retrieving notifications",
                error = ex.Message
            });
        }
    }

    [HttpGet("unread-count")]
    public async Task<IActionResult> GetUnreadCount()
    {
        try
        {
            var userId = GetUserId();
            if (string.IsNullOrEmpty(userId))
                return Unauthorized(new { message = "User ID not found in token" });

            var count = await _notificationService.GetUnreadCountAsync(userId);

            return Ok(new
            {
                unreadCount = count
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting unread count");
            return StatusCode(500, new
            {
                message = "Server error while getting unread count",
                error = ex.Message
            });
        }
    }

    /// <summary>
    /// Đánh dấu 1 notification là đã đọc (không cần createdAt)
    /// </summary>
    [HttpPut("{notificationId}/read")]
    public async Task<IActionResult> MarkAsRead(string notificationId)
    {
        try
        {
            var userId = GetUserId();
            if (string.IsNullOrEmpty(userId))
                return Unauthorized(new { message = "User ID not found in token" });

            var result = await _notificationService.MarkAsReadAsync(userId, notificationId);

            return Ok(new
            {
                message = result.Message,
                data = result
            });
        }
        catch (NotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (UnauthorizedAccessException ex)
        {
            return Forbid();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error marking notification as read");
            return StatusCode(500, new
            {
                message = "Server error while marking notification as read",
                error = ex.Message
            });
        }
    }

    [HttpPut("read-all")]
    public async Task<IActionResult> MarkAllAsRead()
    {
        try
        {
            var userId = GetUserId();
            if (string.IsNullOrEmpty(userId))
                return Unauthorized(new { message = "User ID not found in token" });

            var result = await _notificationService.MarkAllAsReadAsync(userId);

            return Ok(new
            {
                message = result.Message,
                data = result
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error marking all notifications as read");
            return StatusCode(500, new
            {
                message = "Server error while marking all notifications as read",
                error = ex.Message
            });
        }
    }

    /// <summary>
    /// Xóa 1 notification (không cần createdAt)
    /// </summary>
    [HttpDelete("{notificationId}")]
    public async Task<IActionResult> DeleteNotification(string notificationId)
    {
        try
        {
            var userId = GetUserId();
            if (string.IsNullOrEmpty(userId))
                return Unauthorized(new { message = "User ID not found in token" });

            var result = await _notificationService.DeleteNotificationAsync(userId, notificationId);

            return Ok(new
            {
                message = result.Message,
                data = result
            });
        }
        catch (NotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (UnauthorizedAccessException ex)
        {
            return Forbid();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting notification");
            return StatusCode(500, new
            {
                message = "Server error while deleting notification",
                error = ex.Message
            });
        }
    }

    private string? GetUserId()
    {
        return User.FindFirst(ClaimTypes.NameIdentifier)?.Value
               ?? User.FindFirst("sub")?.Value;
    }
}