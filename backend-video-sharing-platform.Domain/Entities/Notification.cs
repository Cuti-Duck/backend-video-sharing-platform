using Amazon.DynamoDBv2.DataModel;

namespace backend_video_sharing_platform.Domain.Entities
{
    [DynamoDBTable("Notifications")]
    public class Notification
    {
        /// <summary>
        /// User nhận notification (Partition Key)
        /// </summary>
        [DynamoDBHashKey("recipientUserId")]
        public string RecipientUserId { get; set; } = string.Empty;

        /// <summary>
        /// Timestamp tạo notification (Sort Key)
        /// Format: ISO 8601 - "2025-01-15T10:30:00Z"
        /// </summary>
        [DynamoDBRangeKey("createdAt")]
        public string CreatedAt { get; set; } = string.Empty;

        /// <summary>
        /// Unique ID của notification
        /// </summary>
        [DynamoDBProperty("notificationId")]
        public string NotificationId { get; set; } = string.Empty;

        /// <summary>
        /// Loại notification: VIDEO_LIKED, VIDEO_COMMENTED, COMMENT_LIKED, COMMENT_REPLIED, NEW_VIDEO_UPLOADED
        /// </summary>
        [DynamoDBProperty("type")]
        public string Type { get; set; } = string.Empty;

        /// <summary>
        /// User thực hiện hành động (người gây ra notification)
        /// </summary>
        [DynamoDBProperty("actorUserId")]
        public string ActorUserId { get; set; } = string.Empty;

        /// <summary>
        /// Tên của người thực hiện hành động
        /// </summary>
        [DynamoDBProperty("actorName")]
        public string ActorName { get; set; } = string.Empty;

        /// <summary>
        /// Avatar URL của người thực hiện hành động
        /// </summary>
        [DynamoDBProperty("actorAvatarUrl")]
        public string? ActorAvatarUrl { get; set; }

        /// <summary>
        /// Video liên quan (nullable - chỉ có khi type là VIDEO_LIKED, VIDEO_COMMENTED, NEW_VIDEO_UPLOADED)
        /// </summary>
        [DynamoDBProperty("videoId")]
        public string? VideoId { get; set; }

        /// <summary>
        /// Video title (để hiển thị notification)
        /// </summary>
        [DynamoDBProperty("videoTitle")]
        public string? VideoTitle { get; set; }

        /// <summary>
        /// Comment liên quan (nullable - chỉ có khi type là COMMENT_LIKED, COMMENT_REPLIED)
        /// </summary>
        [DynamoDBProperty("commentId")]
        public string? CommentId { get; set; }

        /// <summary>
        /// Message hiển thị cho user
        /// Ví dụ: "John Doe liked your video 'My First Video'"
        /// </summary>
        [DynamoDBProperty("message")]
        public string Message { get; set; } = string.Empty;

        /// <summary>
        /// Trạng thái đã đọc - "true" hoặc "false" (string vì DynamoDB GSI)
        /// </summary>
        [DynamoDBProperty("isRead")]
        public string IsRead { get; set; } = "false";

        /// <summary>
        /// Unix timestamp (epoch time) để DynamoDB TTL tự động xóa notification sau 30 ngày
        /// Tính: DateTime.UtcNow.AddDays(30).ToUnixTimeSeconds()
        /// </summary>
        [DynamoDBProperty("ttl")]
        public long Ttl { get; set; }
    }
}