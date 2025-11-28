using Amazon.DynamoDBv2.Model;
using backend_video_sharing_platform.Application.Common.Exceptions;
using backend_video_sharing_platform.Application.DTOs.Notification;
using backend_video_sharing_platform.Application.Interfaces;
using backend_video_sharing_platform.Domain.Entities;
using backend_video_sharing_platform.Domain.Enums;
using Microsoft.Extensions.Logging;
using System.Text;
using System.Text.Json;

namespace backend_video_sharing_platform.Application.Services
{
    public class NotificationService : INotificationService
    {
        private readonly INotificationRepository _notificationRepo;
        private readonly IUserRepository _userRepo;
        private readonly IVideoRepository _videoRepo;
        private readonly ICommentRepository _commentRepo;
        private readonly ISubscriptionRepository _subscriptionRepo;
        private readonly IChannelRepository _channelRepo;
        private readonly ILogger<NotificationService> _logger;

        public NotificationService(
            INotificationRepository notificationRepo,
            IUserRepository userRepo,
            IVideoRepository videoRepo,
            ICommentRepository commentRepo,
            ISubscriptionRepository subscriptionRepo,
            IChannelRepository channelRepo,
            ILogger<NotificationService> logger)
        {
            _notificationRepo = notificationRepo;
            _userRepo = userRepo;
            _videoRepo = videoRepo;
            _commentRepo = commentRepo;
            _subscriptionRepo = subscriptionRepo;
            _channelRepo = channelRepo;
            _logger = logger;
        }

        /// <summary>
        /// Tạo notification mới
        /// </summary>
        public async Task<CreateNotificationResponse> CreateNotificationAsync(CreateNotificationRequest request)
        {
            // 1. Validate: Không tự notify chính mình
            if (request.RecipientUserId == request.ActorUserId)
            {
                return new CreateNotificationResponse
                {
                    Success = false,
                    Message = "Cannot create notification for yourself"
                };
            }

            // 2. Get actor info
            var actor = await _userRepo.GetByIdAsync(request.ActorUserId);
            if (actor == null)
            {
                throw new NotFoundException("Actor user not found");
            }

            // 3. Get recipient info (để verify user tồn tại)
            var recipient = await _userRepo.GetByIdAsync(request.RecipientUserId);
            if (recipient == null)
            {
                throw new NotFoundException("Recipient user not found");
            }

            // 4. Build notification message và get related data
            string message;
            string? videoTitle = null;

            switch (request.Type)
            {
                case nameof(NotificationType.VIDEO_LIKED):
                    var video1 = await _videoRepo.GetByIdAsync(request.VideoId!);
                    videoTitle = video1?.Title;
                    message = $"{actor.Name} liked your video";
                    if (!string.IsNullOrEmpty(videoTitle))
                        message += $" \"{videoTitle}\"";
                    break;

                case nameof(NotificationType.VIDEO_COMMENTED):
                    var video2 = await _videoRepo.GetByIdAsync(request.VideoId!);
                    videoTitle = video2?.Title;
                    message = $"{actor.Name} commented on your video";
                    if (!string.IsNullOrEmpty(videoTitle))
                        message += $" \"{videoTitle}\"";
                    break;

                case nameof(NotificationType.COMMENT_LIKED):
                    message = $"{actor.Name} liked your comment";
                    break;

                case nameof(NotificationType.COMMENT_REPLIED):
                    message = $"{actor.Name} replied to your comment";
                    break;

                case nameof(NotificationType.NEW_VIDEO_UPLOADED):
                    var video3 = await _videoRepo.GetByIdAsync(request.VideoId!);
                    videoTitle = video3?.Title;
                    message = $"{actor.Name} uploaded a new video";
                    if (!string.IsNullOrEmpty(videoTitle))
                        message += $": \"{videoTitle}\"";
                    break;

                default:
                    message = $"{actor.Name} interacted with your content";
                    break;
            }

            // 5. Create notification entity
            var now = DateTime.UtcNow;
            var notification = new Notification
            {
                RecipientUserId = request.RecipientUserId,
                NotificationId = Guid.NewGuid().ToString(),
                CreatedAt = now.ToString("o"), // ISO 8601 format
                Type = request.Type,
                ActorUserId = request.ActorUserId,
                ActorName = actor.Name,
                ActorAvatarUrl = actor.AvatarUrl,
                VideoId = request.VideoId,
                VideoTitle = videoTitle,
                CommentId = request.CommentId,
                Message = message,
                IsRead = "false",
                Ttl = new DateTimeOffset(now.AddDays(30)).ToUnixTimeSeconds() // Auto-delete after 30 days
            };

            // 6. Save to database
            await _notificationRepo.SaveAsync(notification);

            _logger.LogInformation(
                "Created notification {NotificationId} for user {RecipientUserId}: {Type}",
                notification.NotificationId,
                request.RecipientUserId,
                request.Type
            );

            return new CreateNotificationResponse
            {
                NotificationId = notification.NotificationId,
                Success = true,
                Message = "Notification created successfully"
            };
        }

        /// <summary>
        /// Notify tất cả subscribers khi channel đăng video mới
        /// </summary>
        public async Task NotifySubscribersAsync(string channelId, string videoId)
        {
            // 1. Get channel info
            var channel = await _channelRepo.GetByIdAsync(channelId);
            if (channel == null)
            {
                _logger.LogWarning("Channel {ChannelId} not found", channelId);
                return;
            }

            // 2. Get video info
            var video = await _videoRepo.GetByIdAsync(videoId);
            if (video == null)
            {
                _logger.LogWarning("Video {VideoId} not found", videoId);
                return;
            }

            // 3. Get all subscribers
            var subscriptions = await _subscriptionRepo.GetChannelSubscribersAsync(channelId);

            if (subscriptions.Count == 0)
            {
                _logger.LogInformation("Channel {ChannelId} has no subscribers", channelId);
                return;
            }

            // 4. Get channel owner (actor)
            var channelOwner = await _userRepo.GetByIdAsync(channel.UserId);
            if (channelOwner == null)
            {
                _logger.LogWarning("Channel owner {UserId} not found", channel.UserId);
                return;
            }

            // 5. Create notifications for all subscribers
            var notifications = new List<Notification>();
            var now = DateTime.UtcNow;

            foreach (var subscription in subscriptions)
            {
                // Skip nếu subscriber là chính channel owner (edge case)
                if (subscription.UserId == channel.UserId)
                    continue;

                var notification = new Notification
                {
                    RecipientUserId = subscription.UserId,
                    NotificationId = Guid.NewGuid().ToString(),
                    CreatedAt = now.ToString("o"),
                    Type = nameof(NotificationType.NEW_VIDEO_UPLOADED),
                    ActorUserId = channel.UserId,
                    ActorName = channelOwner.Name,
                    ActorAvatarUrl = channelOwner.AvatarUrl,
                    VideoId = videoId,
                    VideoTitle = video.Title,
                    Message = $"{channelOwner.Name} uploaded a new video: \"{video.Title}\"",
                    IsRead = "false",
                    Ttl = new DateTimeOffset(now.AddDays(30)).ToUnixTimeSeconds()
                };

                notifications.Add(notification);
            }

            // 6. Batch save all notifications
            if (notifications.Count > 0)
            {
                await _notificationRepo.BatchSaveAsync(notifications);

                _logger.LogInformation(
                    "Notified {Count} subscribers about new video {VideoId} from channel {ChannelId}",
                    notifications.Count,
                    videoId,
                    channelId
                );
            }
        }

        /// <summary>
        /// Lấy notifications của user
        /// </summary>
        public async Task<GetNotificationsResponse> GetUserNotificationsAsync(
            string userId,
            bool unreadOnly = false,
            int limit = 20,
            string? cursor = null)
        {
            // Validate limit
            if (limit < 1) limit = 20;
            if (limit > 100) limit = 100;

            List<Notification> notifications;
            Dictionary<string, AttributeValue>? lastKey = null;

            if (unreadOnly)
            {
                // Sử dụng GSI để query unread notifications
                notifications = await _notificationRepo.GetUnreadNotificationsAsync(userId, limit);
            }
            else
            {
                // Parse cursor (if provided)
                Dictionary<string, AttributeValue>? lastEvaluatedKey = null;
                if (!string.IsNullOrEmpty(cursor))
                {
                    try
                    {
                        lastEvaluatedKey = DecodeCursor(cursor);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Invalid cursor: {Cursor}", cursor);
                    }
                }

                // Query all notifications with pagination
                var result = await _notificationRepo.GetUserNotificationsAsync(userId, limit, lastEvaluatedKey);
                notifications = result.Items;
                lastKey = result.LastKey;
            }

            // Count unread
            var unreadCount = await _notificationRepo.CountUnreadNotificationsAsync(userId);

            // Map to DTOs
            var notificationDtos = notifications.Select(n => new NotificationDto
            {
                NotificationId = n.NotificationId,
                Type = n.Type,
                ActorUserId = n.ActorUserId,
                ActorName = n.ActorName,
                ActorAvatarUrl = n.ActorAvatarUrl,
                VideoId = n.VideoId,
                VideoTitle = n.VideoTitle,
                CommentId = n.CommentId,
                Message = n.Message,
                IsRead = n.IsRead == "true",
                CreatedAt = n.CreatedAt
            }).ToList();

            return new GetNotificationsResponse
            {
                Notifications = notificationDtos,
                TotalCount = notifications.Count,
                UnreadCount = unreadCount,
                NextCursor = lastKey != null ? EncodeCursor(lastKey) : null
            };
        }

        /// <summary>
        /// Lấy unread count
        /// </summary>
        public async Task<int> GetUnreadCountAsync(string userId)
        {
            return await _notificationRepo.CountUnreadNotificationsAsync(userId);
        }

        /// <summary>
        /// Mark notification as read
        /// </summary>
        public async Task<MarkAsReadResponse> MarkAsReadAsync(string userId, string notificationId)
        {
            // 1. Get notification by notificationId
            var notification = await _notificationRepo.GetByNotificationIdAsync(notificationId);

            if (notification == null)
            {
                throw new NotFoundException("Notification not found");
            }

            // 2. Verify notification belongs to user
            if (notification.RecipientUserId != userId)
            {
                throw new UnauthorizedAccessException("You don't have permission to access this notification");
            }

            // 3. Check if already read
            if (notification.IsRead == "true")
            {
                return new MarkAsReadResponse
                {
                    NotificationId = notificationId,
                    Success = true,
                    Message = "Notification already marked as read"
                };
            }

            // 4. Update
            notification.IsRead = "true";
            await _notificationRepo.UpdateAsync(notification);

            _logger.LogInformation(
                "User {UserId} marked notification {NotificationId} as read",
                userId,
                notificationId
            );

            return new MarkAsReadResponse
            {
                NotificationId = notificationId,
                Success = true,
                Message = "Notification marked as read"
            };
        }

        /// <summary>
        /// Mark all notifications as read
        /// </summary>
        public async Task<MarkAsReadResponse> MarkAllAsReadAsync(string userId)
        {
            await _notificationRepo.MarkAllAsReadAsync(userId);

            _logger.LogInformation("User {UserId} marked all notifications as read", userId);

            return new MarkAsReadResponse
            {
                Success = true,
                Message = "All notifications marked as read"
            };
        }

        /// <summary>
        /// Xóa notification
        /// </summary>
        public async Task<DeleteNotificationResponse> DeleteNotificationAsync(
         string userId,
         string notificationId)
        {
            // 1. Get notification by notificationId
            var notification = await _notificationRepo.GetByNotificationIdAsync(notificationId);

            if (notification == null)
            {
                throw new NotFoundException("Notification not found");
            }

            // 2. Verify notification belongs to user
            if (notification.RecipientUserId != userId)
            {
                throw new UnauthorizedAccessException("You don't have permission to delete this notification");
            }

            // 3. Delete
            await _notificationRepo.DeleteByNotificationIdAsync(notificationId);

            _logger.LogInformation(
                "User {UserId} deleted notification {NotificationId}",
                userId,
                notificationId
            );

            return new DeleteNotificationResponse
            {
                NotificationId = notificationId,
                Success = true,
                Message = "Notification deleted successfully"
            };
        }

        #region Helper Methods

        /// <summary>
        /// Encode DynamoDB LastEvaluatedKey to Base64 cursor
        /// </summary>
        private string EncodeCursor(Dictionary<string, AttributeValue> lastKey)
        {
            var json = JsonSerializer.Serialize(lastKey);
            var bytes = Encoding.UTF8.GetBytes(json);
            return Convert.ToBase64String(bytes);
        }

        /// <summary>
        /// Decode Base64 cursor to DynamoDB LastEvaluatedKey
        /// </summary>
        private Dictionary<string, AttributeValue> DecodeCursor(string cursor)
        {
            var bytes = Convert.FromBase64String(cursor);
            var json = Encoding.UTF8.GetString(bytes);
            return JsonSerializer.Deserialize<Dictionary<string, AttributeValue>>(json)
                   ?? throw new InvalidOperationException("Invalid cursor format");
        }

        #endregion
    }
}