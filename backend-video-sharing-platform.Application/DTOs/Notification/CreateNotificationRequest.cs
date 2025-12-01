namespace backend_video_sharing_platform.Application.DTOs.Notification
{
    /// <summary>
    /// Request để tạo notification mới
    /// </summary>
    public class CreateNotificationRequest
    {
        /// <summary>
        /// User nhận notification
        /// </summary>
        public string RecipientUserId { get; set; } = string.Empty;

        /// <summary>
        /// User thực hiện hành động
        /// </summary>
        public string ActorUserId { get; set; } = string.Empty;

        /// <summary>
        /// Loại notification
        /// </summary>
        public string Type { get; set; } = string.Empty;

        /// <summary>
        /// Video liên quan (optional)
        /// </summary>
        public string? VideoId { get; set; }

        /// <summary>
        /// Comment liên quan (optional)
        /// </summary>
        public string? CommentId { get; set; }
    }

    /// <summary>
    /// Response khi tạo notification thành công
    /// </summary>
    public class CreateNotificationResponse
    {
        public string NotificationId { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public bool Success { get; set; }
    }

    /// <summary>
    /// DTO hiển thị notification cho client
    /// </summary>
    // File: Application/DTOs/Notification/NotificationDto.cs
    public class NotificationDto
    {
        public string NotificationId { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public string ActorUserId { get; set; } = string.Empty;
        public string ActorName { get; set; } = string.Empty;
        public string? ActorAvatarUrl { get; set; }
        public string? VideoId { get; set; }
        public string? VideoTitle { get; set; }
        public string? VideoThumbnailUrl { get; set; } // ✅ NEW
        public string? CommentId { get; set; }
        public string Message { get; set; } = string.Empty;
        public bool IsRead { get; set; }
        public string CreatedAt { get; set; } = string.Empty;
    }

    /// <summary>
    /// Response khi lấy danh sách notifications
    /// </summary>
    public class GetNotificationsResponse
    {
        public List<NotificationDto> Notifications { get; set; } = new();
        public int TotalCount { get; set; }
        public int UnreadCount { get; set; }
        public string? NextCursor { get; set; } // For pagination
    }

    /// <summary>
    /// Response khi mark as read
    /// </summary>
    public class MarkAsReadResponse
    {
        public string NotificationId { get; set; } = string.Empty;
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
    }

    /// <summary>
    /// Response khi xóa notification
    /// </summary>
    public class DeleteNotificationResponse
    {
        public string NotificationId { get; set; } = string.Empty;
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
    }

    /// <summary>
    /// Response cho unread count
    /// </summary>
    public class UnreadCountResponse
    {
        public int UnreadCount { get; set; }
    }
}