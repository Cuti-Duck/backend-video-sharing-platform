using System.Security.Claims;
using backend_video_sharing_platform.Application.DTOs.Subscription;
using backend_video_sharing_platform.Application.Interfaces;
using backend_video_sharing_platform.Application.Services;
using backend_video_sharing_platform.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/subscriptions")]
public class SubscriptionController : ControllerBase
{
    private readonly ISubscriptionService _service;

    public SubscriptionController(ISubscriptionService service)
    {
        _service = service;
    }

    [HttpPost("subscribe")]
    [Authorize]
    public async Task<IActionResult> Subscribe([FromBody] SubscribeRequest request)
    {
        // Get userId from authenticated user claims
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                     ?? User.FindFirst("sub")?.Value;

        if (string.IsNullOrEmpty(userId))
            return Unauthorized("User ID not found in token");

        var result = await _service.SubscribeAsync(userId, request);
        return Ok(result);
    }

    [HttpPost("unsubscribe")]
    [Authorize]
    public async Task<IActionResult> Unsubscribe([FromBody] UnsubscribeRequest request)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                     ?? User.FindFirst("sub")?.Value;

        if (string.IsNullOrEmpty(userId))
            return Unauthorized("User ID not found in token");

        var result = await _service.UnsubscribeAsync(userId, request);
        return Ok(result);
    }

    [HttpGet("mysubscribedchannel")]
    [Authorize]
    public async Task<IActionResult> GetMySubscriptions()
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                     ?? User.FindFirst("sub")?.Value;

        if (string.IsNullOrEmpty(userId))
            return Unauthorized("User ID not found in token");

        var result = await _service.GetMySubscriptionsAsync(userId);
        return Ok(result);
    }

    [HttpGet("channel/{channelId}/subscribers")]
    public async Task<IActionResult> GetChannelSubscribers(string channelId)
    {
        var result = await _service.GetChannelSubscribersAsync(channelId);
        return Ok(result);
    }
}
