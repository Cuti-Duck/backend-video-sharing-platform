using backend_video_sharing_platform.Application.DTOs.Notification;

namespace backend_video_sharing_platform.Application.Interfaces
{
    /// <summary>
    /// Service interface cho Notification business logic
    /// </summary>
    public interface INotificationService
    {
        /// <summary>
        /// Tạo notification mới cho 1 user
        /// </summary>
        Task<CreateNotificationResponse> CreateNotificationAsync(CreateNotificationRequest request);

        /// <summary>
        /// Notify tất cả subscribers của channel khi có video mới
        /// </summary>
        Task NotifySubscribersAsync(string channelId, string videoId);

        /// <summary>
        /// Lấy danh sách notifications của user (với pagination)
        /// </summary>
        Task<GetNotificationsResponse> GetUserNotificationsAsync(
            string userId,
            bool unreadOnly = false,
            int limit = 20,
            string? cursor = null
        );

        /// <summary>
        /// Đếm số notifications chưa đọc
        /// </summary>
        Task<int> GetUnreadCountAsync(string userId);

        /// <summary>
        /// Mark 1 notification là đã đọc
        /// </summary>
        Task<MarkAsReadResponse> MarkAsReadAsync(string userId, string notificationId);

        /// <summary>
        /// Mark tất cả notifications là đã đọc
        /// </summary>
        Task<MarkAsReadResponse> MarkAllAsReadAsync(string userId);

        /// <summary>
        /// Xóa 1 notification
        /// </summary>
        Task<DeleteNotificationResponse> DeleteNotificationAsync(string userId, string notificationId);
    }
}