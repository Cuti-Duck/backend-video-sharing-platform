using Amazon.DynamoDBv2.DataModel;
namespace backend_video_sharing_platform.Domain.Entities
{
    [DynamoDBTable("VideoLikes")]
    public class VideoLike
    {
        [DynamoDBHashKey("userId")]
        public string UserId { get; set; } = string.Empty;

        [DynamoDBRangeKey("videoId")]
        public string VideoId { get; set; } = string.Empty;

        [DynamoDBProperty("createdAt")]
        public string CreatedAt { get; set; } = string.Empty;
    }
}