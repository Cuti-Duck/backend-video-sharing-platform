using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2.DocumentModel;
using Amazon.DynamoDBv2.Model;
using backend_video_sharing_platform.Application.Interfaces;
using backend_video_sharing_platform.Domain.Entities;
using Microsoft.Extensions.Logging;

namespace backend_video_sharing_platform.Infrastructure.Repositories
{
    public class NotificationRepository : INotificationRepository
    {
        private readonly IDynamoDBContext _context;
        private readonly IAmazonDynamoDB _client;
        private readonly ILogger<NotificationRepository> _logger;
        private const string TABLE_NAME = "Notifications";
        private const string GSI_NAME = "UnreadNotificationsIndex";
        private const string GSI_NOTIFICATION_ID = "NotificationIdIndex";

        public NotificationRepository(
            IDynamoDBContext context,
            IAmazonDynamoDB client,
            ILogger<NotificationRepository> logger)
        {
            _context = context;
            _client = client;
            _logger = logger;
        }
        public async Task<Notification?> GetByNotificationIdAsync(string notificationId)
        {
            var request = new QueryRequest
            {
                TableName = TABLE_NAME,
                IndexName = GSI_NOTIFICATION_ID,
                KeyConditionExpression = "notificationId = :notificationId",
                ExpressionAttributeValues = new Dictionary<string, AttributeValue>
            {
                { ":notificationId", new AttributeValue { S = notificationId } }
            },
                Limit = 1
            };

            var response = await _client.QueryAsync(request);

            if (response.Items.Count == 0)
                return null;

            return MapAttributesToNotification(response.Items[0]);
        }

        public async Task DeleteByNotificationIdAsync(string notificationId)
        {
            // 1. Query để lấy PK và SK
            var notification = await GetByNotificationIdAsync(notificationId);

            if (notification == null)
            {
                _logger.LogWarning("Notification {NotificationId} not found", notificationId);
                return;
            }

            // 2. Delete bằng PK và SK
            await _context.DeleteAsync<Notification>(notification.RecipientUserId, notification.CreatedAt);
        }
        /// <summary>
        /// Lấy 1 notification theo PK và SK
        /// </summary>
        public Task<Notification?> GetAsync(string recipientUserId, string createdAt)
        {
            return _context.LoadAsync<Notification>(recipientUserId, createdAt);
        }

        /// <summary>
        /// Lưu notification mới
        /// </summary>
        public Task SaveAsync(Notification notification)
        {
            return _context.SaveAsync(notification);
        }

        /// <summary>
        /// Update notification (mark as read)
        /// </summary>
        public Task UpdateAsync(Notification notification)
        {
            return _context.SaveAsync(notification);
        }

        /// <summary>
        /// Xóa notification
        /// </summary>
        public Task DeleteAsync(string recipientUserId, string createdAt)
        {
            return _context.DeleteAsync<Notification>(recipientUserId, createdAt);
        }

        /// <summary>
        /// Lấy notifications của user với pagination
        /// </summary>
        public async Task<(List<Notification> Items, Dictionary<string, AttributeValue>? LastKey)>
            GetUserNotificationsAsync(
                string recipientUserId,
                int limit,
                Dictionary<string, AttributeValue>? lastEvaluatedKey = null)
        {
            var request = new QueryRequest
            {
                TableName = TABLE_NAME,
                KeyConditionExpression = "recipientUserId = :userId",
                ExpressionAttributeValues = new Dictionary<string, AttributeValue>
                {
                    { ":userId", new AttributeValue { S = recipientUserId } }
                },
                ScanIndexForward = false, // Sort descending (mới nhất trước)
                Limit = limit,
                ExclusiveStartKey = lastEvaluatedKey
            };

            var response = await _client.QueryAsync(request);

            var notifications = new List<Notification>();
            foreach (var item in response.Items)
            {
                notifications.Add(MapAttributesToNotification(item));
            }

            return (notifications, response.LastEvaluatedKey?.Count > 0 ? response.LastEvaluatedKey : null);
        }

        /// <summary>
        /// Lấy chỉ notifications chưa đọc (sử dụng GSI)
        /// </summary>
        public async Task<List<Notification>> GetUnreadNotificationsAsync(string recipientUserId, int limit = 20)
        {
            var request = new QueryRequest
            {
                TableName = TABLE_NAME,
                IndexName = GSI_NAME, // UnreadNotificationsIndex
                KeyConditionExpression = "recipientUserId = :userId AND isRead = :isRead",
                ExpressionAttributeValues = new Dictionary<string, AttributeValue>
                {
                    { ":userId", new AttributeValue { S = recipientUserId } },
                    { ":isRead", new AttributeValue { S = "false" } }
                },
                ScanIndexForward = false, // Mới nhất trước
                Limit = limit
            };

            var response = await _client.QueryAsync(request);

            var notifications = new List<Notification>();
            foreach (var item in response.Items)
            {
                notifications.Add(MapAttributesToNotification(item));
            }

            return notifications;
        }

        /// <summary>
        /// Đếm số notifications chưa đọc
        /// </summary>
        public async Task<int> CountUnreadNotificationsAsync(string recipientUserId)
        {
            var request = new QueryRequest
            {
                TableName = TABLE_NAME,
                IndexName = GSI_NAME,
                KeyConditionExpression = "recipientUserId = :userId AND isRead = :isRead",
                ExpressionAttributeValues = new Dictionary<string, AttributeValue>
                {
                    { ":userId", new AttributeValue { S = recipientUserId } },
                    { ":isRead", new AttributeValue { S = "false" } }
                },
                Select = "COUNT" // Chỉ đếm, không lấy data
            };

            var response = await _client.QueryAsync(request);
            return (int)response.Count;
        }

        /// <summary>
        /// Mark tất cả notifications của user thành đã đọc
        /// </summary>
        public async Task MarkAllAsReadAsync(string recipientUserId)
        {
            // 1. Query all unread notifications
            var unreadNotifications = await GetUnreadNotificationsAsync(recipientUserId, 100);

            // 2. Update từng notification
            foreach (var notification in unreadNotifications)
            {
                notification.IsRead = "true";
                await UpdateAsync(notification);
            }

            _logger.LogInformation(
                "Marked {Count} notifications as read for user {UserId}",
                unreadNotifications.Count,
                recipientUserId
            );
        }

        /// <summary>
        /// Batch save nhiều notifications (dùng cho notify subscribers)
        /// </summary>
        public async Task BatchSaveAsync(List<Notification> notifications)
        {
            if (notifications == null || notifications.Count == 0)
                return;

            // DynamoDB BatchWriteItem max 25 items per request
            const int batchSize = 25;

            for (int i = 0; i < notifications.Count; i += batchSize)
            {
                var batch = notifications.Skip(i).Take(batchSize).ToList();
                var batchWrite = _context.CreateBatchWrite<Notification>();

                foreach (var notification in batch)
                {
                    batchWrite.AddPutItem(notification);
                }

                await batchWrite.ExecuteAsync();

                _logger.LogInformation(
                    "Batch saved {Count} notifications (batch {BatchNum})",
                    batch.Count,
                    i / batchSize + 1
                );
            }
        }

        /// <summary>
        /// Helper: Map DynamoDB AttributeValue dictionary to Notification object
        /// </summary>
        private Notification MapAttributesToNotification(Dictionary<string, AttributeValue> item)
        {
            return new Notification
            {
                RecipientUserId = item.ContainsKey("recipientUserId") ? item["recipientUserId"].S : string.Empty,
                CreatedAt = item.ContainsKey("createdAt") ? item["createdAt"].S : string.Empty,
                NotificationId = item.ContainsKey("notificationId") ? item["notificationId"].S : string.Empty,
                Type = item.ContainsKey("type") ? item["type"].S : string.Empty,
                ActorUserId = item.ContainsKey("actorUserId") ? item["actorUserId"].S : string.Empty,
                ActorName = item.ContainsKey("actorName") ? item["actorName"].S : string.Empty,
                ActorAvatarUrl = item.ContainsKey("actorAvatarUrl") ? item["actorAvatarUrl"].S : null,
                VideoId = item.ContainsKey("videoId") ? item["videoId"].S : null,
                VideoTitle = item.ContainsKey("videoTitle") ? item["videoTitle"].S : null,
                VideoThumbnailUrl = item.ContainsKey("videoThumbnailUrl") ? item["videoThumbnailUrl"].S : null, // ✅ NEW
                CommentId = item.ContainsKey("commentId") ? item["commentId"].S : null,
                Message = item.ContainsKey("message") ? item["message"].S : string.Empty,
                IsRead = item.ContainsKey("isRead") ? item["isRead"].S : "false",
                Ttl = item.ContainsKey("ttl") && long.TryParse(item["ttl"].N, out var ttl) ? ttl : 0
            };
        }
    }
}