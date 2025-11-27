using Amazon.DynamoDBv2.DataModel;

namespace backend_video_sharing_platform.Domain.Entities
{
    [DynamoDBTable("Comments")]
    public class Comment
    {
        // Partition Key: videoId (để query tất cả comments của 1 video)
        [DynamoDBHashKey("videoId")]
        public string VideoId { get; set; } = string.Empty;

        // Sort Key: commentId (unique identifier cho mỗi comment)
        [DynamoDBRangeKey("commentId")]
        public string CommentId { get; set; } = string.Empty;

        // User information
        [DynamoDBProperty("userId")]
        public string UserId { get; set; } = string.Empty;

        [DynamoDBProperty("userName")]
        public string UserName { get; set; } = string.Empty;

        [DynamoDBProperty("userAvatarUrl")]
        public string? UserAvatarUrl { get; set; }

        // Comment content
        [DynamoDBProperty("content")]
        public string Content { get; set; } = string.Empty;

        // Parent comment (null nếu là comment gốc, có giá trị nếu là reply)
        [DynamoDBProperty("parentCommentId")]
        public string? ParentCommentId { get; set; }

        // Metrics
        [DynamoDBProperty("likeCount")]
        public int LikeCount { get; set; } = 0;

        [DynamoDBProperty("replyCount")]
        public int ReplyCount { get; set; } = 0;

        // Status
        [DynamoDBProperty("isEdited")]
        public bool IsEdited { get; set; } = false;

        [DynamoDBProperty("isDeleted")]
        public bool IsDeleted { get; set; } = false;

        // Timestamps
        [DynamoDBProperty("createdAt")]
        public string CreatedAt { get; set; } = string.Empty;

        [DynamoDBProperty("updatedAt")]
        public string? UpdatedAt { get; set; }
    }
}