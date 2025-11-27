using Amazon.DynamoDBv2.DataModel;

namespace backend_video_sharing_platform.Domain.Entities
{
    [DynamoDBTable("CommentLikes")]
    public class CommentLike
    {
        [DynamoDBHashKey("commentId")]
        public string CommentId { get; set; } = string.Empty;

        [DynamoDBRangeKey("userId")]
        public string UserId { get; set; } = string.Empty;

        [DynamoDBProperty("createdAt")]
        public string CreatedAt { get; set; } = string.Empty;
    }
}
