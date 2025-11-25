using Amazon.DynamoDBv2.DataModel;

namespace backend_video_sharing_platform.Domain.Entities
{
    [DynamoDBTable("Subscriptions")]
    public class Subscription
    {
        [DynamoDBHashKey("userId")]
        public string UserId { get; set; } = string.Empty;

        [DynamoDBRangeKey("channelId")]
        public string ChannelId { get; set; } = string.Empty;

        [DynamoDBProperty("createdAt")]
        public string CreatedAt { get; set; } = string.Empty;
    }
}
