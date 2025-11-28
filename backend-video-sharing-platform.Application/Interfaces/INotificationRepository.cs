using backend_video_sharing_platform.Domain.Entities;

namespace backend_video_sharing_platform.Application.Interfaces
{
    /// <summary>
    /// Repository interface cho Notifications
    /// </summary>
    public interface INotificationRepository
    {
        /// <summary>
        /// Lấy 1 notification theo recipientUserId và createdAt
        /// </summary>
        Task<Notification?> GetAsync(string recipientUserId, string createdAt);

        /// <summary>
        /// Lưu notification mới
        /// </summary>
        Task SaveAsync(Notification notification);

        /// <summary>
        /// Update notification (thường dùng để mark as read)
        /// </summary>
        Task UpdateAsync(Notification notification);

        /// <summary>
        /// Xóa notification
        /// </summary>
        Task DeleteAsync(string recipientUserId, string createdAt);

        /// <summary>
        /// Lấy tất cả notifications của 1 user (có pagination)
        /// </summary>
        /// <param name="recipientUserId">User nhận notification</param>
        /// <param name="limit">Số lượng items trả về</param>
        /// <param name="lastEvaluatedKey">Cursor cho pagination (optional)</param>
        Task<(List<Notification> Items, Dictionary<string, Amazon.DynamoDBv2.Model.AttributeValue>? LastKey)>
            GetUserNotificationsAsync(string recipientUserId, int limit, Dictionary<string, Amazon.DynamoDBv2.Model.AttributeValue>? lastEvaluatedKey = null);

        /// <summary>
        /// Lấy chỉ notifications chưa đọc của user (sử dụng GSI)
        /// </summary>
        Task<List<Notification>> GetUnreadNotificationsAsync(string recipientUserId, int limit = 20);

        /// <summary>
        /// Đếm số notifications chưa đọc của user
        /// </summary>
        Task<int> CountUnreadNotificationsAsync(string recipientUserId);

        /// <summary>
        /// Mark tất cả notifications của user thành đã đọc
        /// </summary>
        Task MarkAllAsReadAsync(string recipientUserId);

        /// <summary>
        /// Batch save nhiều notifications cùng lúc (dùng cho notify subscribers)
        /// </summary>
        Task BatchSaveAsync(List<Notification> notifications);

        Task DeleteByNotificationIdAsync(string notificationId);

        Task<Notification?> GetByNotificationIdAsync(string notificationId);
    }
}