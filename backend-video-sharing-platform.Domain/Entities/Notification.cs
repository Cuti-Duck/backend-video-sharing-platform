// File: Domain/Entities/Notification.cs
using Amazon.DynamoDBv2.DataModel;

[DynamoDBTable("Notifications")]
public class Notification
{
    [DynamoDBHashKey("recipientUserId")]
    public string RecipientUserId { get; set; } = string.Empty;

    [DynamoDBRangeKey("createdAt")]
    public string CreatedAt { get; set; } = string.Empty;

    [DynamoDBProperty("notificationId")]
    public string NotificationId { get; set; } = string.Empty;

    [DynamoDBProperty("type")]
    public string Type { get; set; } = string.Empty;

    [DynamoDBProperty("actorUserId")]
    public string ActorUserId { get; set; } = string.Empty;

    [DynamoDBProperty("actorName")]
    public string ActorName { get; set; } = string.Empty;

    [DynamoDBProperty("actorAvatarUrl")]
    public string? ActorAvatarUrl { get; set; }

    [DynamoDBProperty("videoId")]
    public string? VideoId { get; set; }

    [DynamoDBProperty("videoTitle")]
    public string? VideoTitle { get; set; }

    [DynamoDBProperty("videoThumbnailUrl")] // ✅ NEW
    public string? VideoThumbnailUrl { get; set; }

    [DynamoDBProperty("commentId")]
    public string? CommentId { get; set; }

    [DynamoDBProperty("message")]
    public string Message { get; set; } = string.Empty;

    [DynamoDBProperty("isRead")]
    public string IsRead { get; set; } = "false";

    [DynamoDBProperty("ttl")]
    public long Ttl { get; set; }
}