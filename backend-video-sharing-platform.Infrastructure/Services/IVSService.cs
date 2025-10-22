using Amazon;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using Amazon.IVS;
using Amazon.IVS.Model;
using backend_video_sharing_platform.Application.Interfaces;
using Microsoft.Extensions.Configuration;

namespace backend_video_sharing_platform.Infrastructure.Services
{
    public class IVSService : IIVSService
    {
        private readonly IAmazonDynamoDB _channelDynamo; // Channels (Singapore)
        private readonly IAmazonDynamoDB _videoDynamo;   // Videos (Tokyo)
        private readonly IConfiguration _config;
        private readonly AmazonIVSClient _ivs;

        public IVSService(IConfiguration config)
        {
            _config = config;

            // DynamoDB Channels ở Singapore
            var channelRegion = RegionEndpoint.GetBySystemName(_config["AWS:DynamoDB:ChannelsRegion"] ?? "ap-southeast-1");
            _channelDynamo = new AmazonDynamoDBClient(channelRegion);

            // DynamoDB Videos ở Tokyo
            var videoRegion = RegionEndpoint.GetBySystemName(_config["AWS:DynamoDB:VideosRegion"] ?? "ap-northeast-1");
            _videoDynamo = new AmazonDynamoDBClient(videoRegion);

            // IVS ở Tokyo
            _ivs = new AmazonIVSClient(RegionEndpoint.APNortheast1);
        }

        public async Task<object> CreateChannelAsync(string userId)
        {
            var tableName = _config["AWS:DynamoDB:ChannelsTable"];
            var recordArn = _config["AWS:RecordingConfigurationArn"];

            // 1) Check tồn tại
            var check = await _channelDynamo.GetItemAsync(new GetItemRequest
            {
                TableName = tableName,
                Key = new Dictionary<string, AttributeValue> { ["UserId"] = new AttributeValue(userId) }
            });

            if (check.Item?.Count > 0)
            {
                
                string streamKeyValue = null;
                if (check.Item.ContainsKey("StreamKeyArn"))
                {
                    var getKey = await _ivs.GetStreamKeyAsync(new Amazon.IVS.Model.GetStreamKeyRequest
                    {
                        Arn = check.Item["StreamKeyArn"].S
                    });
                    streamKeyValue = getKey.StreamKey?.Value;
                }

                var ingestEndpointHost = check.Item["IngestEndpoint"].S;
                var ingestServer = $"rtmps://{ingestEndpointHost}:443/app/";

                return new
                {
                    message = "Channel already exists",
                    channelArn = check.Item["ChannelArn"].S,
                    playbackUrl = check.Item["PlaybackUrl"].S,
                    ingestEndpoint = ingestEndpointHost,     
                    ingestServer,
                    streamKeyArn = check.Item.ContainsKey("StreamKeyArn") ? check.Item["StreamKeyArn"].S : null,
                    streamKey = streamKeyValue     
                };
            }

            // 2) Tạo mới
            var create = await _ivs.CreateChannelAsync(new CreateChannelRequest
            {
                Name = $"user-{userId}-channel",
                Type = ChannelType.STANDARD,
                Authorized = false,
                LatencyMode = ChannelLatencyMode.LOW,
                RecordingConfigurationArn = recordArn,
                Tags = new Dictionary<string, string>
                {
                    ["UserId"] = userId,
                    ["Project"] = "VideoSharing",
                    ["Env"] = "Prod"
                }
            });

            // Lưu DB
            var now = DateTime.UtcNow.ToString("o");
            await _channelDynamo.PutItemAsync(new PutItemRequest
            {
                TableName = tableName,
                Item = new Dictionary<string, AttributeValue>
                {
                    ["UserId"] = new AttributeValue(userId),
                    ["ChannelArn"] = new AttributeValue(create.Channel.Arn),
                    ["PlaybackUrl"] = new AttributeValue(create.Channel.PlaybackUrl),
                    ["IngestEndpoint"] = new AttributeValue(create.Channel.IngestEndpoint),
                    ["StreamKeyArn"] = new AttributeValue(create.StreamKey.Arn),
                    ["Status"] = new AttributeValue("Active"),
                    ["CreatedAt"] = new AttributeValue(now)
                }
            });

            var ingestServerNew = $"rtmps://{create.Channel.IngestEndpoint}:443/app/";

            return new
            {
                message = "Channel created successfully",
                channelArn = create.Channel.Arn,
                playbackUrl = create.Channel.PlaybackUrl,
                ingestEndpoint = create.Channel.IngestEndpoint,
                ingestServer = ingestServerNew,   
                streamKeyArn = create.StreamKey.Arn,
                streamKey = create.StreamKey.Value 
            };
        }


        public async Task<IEnumerable<object>> GetVideosByUserIdAsync(string userId)
        {
            var tableName = _config["AWS:DynamoDB:VideosTable"];

            var request = new QueryRequest
            {
                TableName = tableName,
                IndexName = "user_id-index",
                KeyConditionExpression = "user_id = :uid",
                ExpressionAttributeValues = new Dictionary<string, AttributeValue>
                {
                    { ":uid", new AttributeValue { S = userId } }
                }
            };

            var response = await _videoDynamo.QueryAsync(request);

            if (response.Items.Count == 0)
                return Enumerable.Empty<object>();

            return response.Items.Select(item => new
            {
                video_id = item["video_id"].S,
                user_id = item["user_id"].S,
                title = item.ContainsKey("title") ? item["title"].S : null,
                status = item.ContainsKey("status") ? item["status"].S : null,
                created_at = item.ContainsKey("created_at") ? item["created_at"].S : null,
                duration_ms = item.ContainsKey("duration_ms") ? item["duration_ms"].N : "0",
                video_url = item.ContainsKey("video_url") ? item["video_url"].S : null
            });
        }
    }
}
