using Amazon.DynamoDBv2.DataModel;

namespace backend_video_sharing_platform.Domain.Entities
{
    [DynamoDBTable("Channels")]
    public class Channel
    {
        [DynamoDBHashKey("channelId")]
        public string ChannelId { get; set; } = string.Empty;

        [DynamoDBProperty("userId")]
        public string UserId { get; set; } = string.Empty;

        [DynamoDBProperty("name")]
        public string Name { get; set; } = string.Empty;

        [DynamoDBProperty("description")]
        public string Description { get; set; } = string.Empty;

        [DynamoDBProperty("subscriberCount")]
        public int SubscriberCount { get; set; }

        [DynamoDBProperty("videoCount")]
        public int VideoCount { get; set; }

        [DynamoDBProperty("createdAt")]
        public string CreatedAt { get; set; } = string.Empty;

        
        [DynamoDBProperty("channelArn")]
        public string? ChannelArn { get; set; }

        [DynamoDBProperty("playbackUrl")]
        public string? PlaybackUrl { get; set; }

        [DynamoDBProperty("ingestEndpoint")]
        public string? IngestEndpoint { get; set; }

        [DynamoDBProperty("streamKeyArn")]
        public string? StreamKeyArn { get; set; }

        [DynamoDBProperty("isLive")]
        public bool IsLive { get; set; } = false;

        [DynamoDBProperty("currentStreamId")]
        public string? CurrentStreamId { get; set; }
    }
}
