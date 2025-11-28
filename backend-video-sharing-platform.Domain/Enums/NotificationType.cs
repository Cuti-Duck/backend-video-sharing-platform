namespace backend_video_sharing_platform.Domain.Enums
{
    /// <summary>
    /// Các loại notification trong hệ thống
    /// </summary>
    public enum NotificationType
    {
        /// <summary>
        /// User A liked video của User B
        /// </summary>
        VIDEO_LIKED,

        /// <summary>
        /// User A commented on video của User B
        /// </summary>
        VIDEO_COMMENTED,

        /// <summary>
        /// User A liked comment của User B
        /// </summary>
        COMMENT_LIKED,

        /// <summary>
        /// User A replied to comment của User B
        /// </summary>
        COMMENT_REPLIED,

        /// <summary>
        /// Channel mà User A subscribe đăng video mới
        /// </summary>
        NEW_VIDEO_UPLOADED
    }
}