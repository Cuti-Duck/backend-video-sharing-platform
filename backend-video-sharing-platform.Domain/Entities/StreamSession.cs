using Amazon.DynamoDBv2.DataModel;

namespace backend_video_sharing_platform.Domain.Entities
{
    [DynamoDBTable("StreamSessions")]
    public class StreamSession
    {
        [DynamoDBHashKey("streamId")]
        public string StreamId { get; set; } = string.Empty;

        [DynamoDBProperty("channelId")]
        public string ChannelId { get; set; } = string.Empty;

        [DynamoDBProperty("userId")]
        public string UserId { get; set; } = string.Empty;

        [DynamoDBProperty("title")]
        public string Title { get; set; } = string.Empty;

        [DynamoDBProperty("description")]
        public string? Description { get; set; }

        [DynamoDBProperty("thumbnailUrl")]
        public string? ThumbnailUrl { get; set; }

        [DynamoDBProperty("startedAt")]
        public string? StartedAt { get; set; }

        [DynamoDBProperty("endedAt")]
        public string? EndedAt { get; set; }

        [DynamoDBProperty("isLive")]
        public int IsLive { get; set; } = 0; // 0 = not started, 1 = live

        [DynamoDBProperty("status")]
        public string Status { get; set; } = "PENDING"; // PENDING, LIVE, ENDED, RECORDED

        [DynamoDBProperty("viewerCount")]
        public int ViewerCount { get; set; } = 0;

        [DynamoDBProperty("recordingUrl")]
        public string? RecordingUrl { get; set; }

        [DynamoDBProperty("createdAt")]
        public string CreatedAt { get; set; } = string.Empty;

        [DynamoDBProperty("updatedAt")]
        public string UpdatedAt { get; set; } = string.Empty;
    }
}