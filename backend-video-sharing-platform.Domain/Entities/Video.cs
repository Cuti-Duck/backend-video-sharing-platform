using Amazon.DynamoDBv2.DataModel;

namespace backend_video_sharing_platform.Domain.Entities
{
    [DynamoDBTable("Videos")]
    public class Video
    {
        // Partition Key
        [DynamoDBHashKey("videoId")]
        public string VideoId { get; set; } = string.Empty;

        [DynamoDBProperty("channelId")]
        public string ChannelId { get; set; } = string.Empty;

        [DynamoDBProperty("userId")]
        public string UserId { get; set; } = string.Empty;

        [DynamoDBProperty("title")]
        public string Title { get; set; } = string.Empty;

        [DynamoDBProperty("description")]
        public string Description { get; set; } = string.Empty;

        [DynamoDBProperty("status")]
        public string Status { get; set; } = string.Empty;  
        // UPLOADING | PROCESSING | COMPLETE | FAILED

        [DynamoDBProperty("type")]
        public string Type { get; set; } = string.Empty;  
        // upload | vod | stream-recording

        [DynamoDBProperty("playbackUrl")]
        public string? PlaybackUrl { get; set; }

        [DynamoDBProperty("key")]
        public string? Key { get; set; } 
        // S3 processed 1080p key

        [DynamoDBProperty("thumbnailUrl")]
        public string? ThumbnailUrl { get; set; }

        [DynamoDBProperty("duration")]
        public double Duration { get; set; }

        [DynamoDBProperty("viewCount")]
        public long ViewCount { get; set; }

        [DynamoDBProperty("likeCount")]
        public long LikeCount { get; set; }

        [DynamoDBProperty("createdAt")]
        public string CreatedAt { get; set; } = string.Empty;

        [DynamoDBProperty("updatedAt")]
        public string UpdatedAt { get; set; } = string.Empty;

        [DynamoDBProperty("commentCount")]
        public int CommentCount { get; set; } = 0;
    }
}
