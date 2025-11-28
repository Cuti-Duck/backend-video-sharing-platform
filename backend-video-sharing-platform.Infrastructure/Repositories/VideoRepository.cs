using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2.Model;
using backend_video_sharing_platform.Application.Interfaces;
using backend_video_sharing_platform.Domain.Entities;

public class VideoRepository : IVideoRepository
{
    private readonly IDynamoDBContext _context;
    private readonly IAmazonDynamoDB _client;

    public VideoRepository(IDynamoDBContext context, IAmazonDynamoDB client)
    {
        _context = context;
        _client = client;
    }

    public async Task<List<Video>> GetAllVideosAsync()
    {
        var conditions = new List<ScanCondition>();
        return await _context.ScanAsync<Video>(conditions).GetRemainingAsync();
    }

    public async Task<List<Video>> GetVideosByChannelIdAsync(string channelId)
    {
        var conditions = new List<ScanCondition>
    {
        new ScanCondition("ChannelId", Amazon.DynamoDBv2.DocumentModel.ScanOperator.Equal, channelId)
        // ^ Chữ C viết hoa - khớp với property name trong class Video
    };
        return await _context.ScanAsync<Video>(conditions).GetRemainingAsync();
    }

    public async Task UpdateThumbnailAsync(string videoId, string thumbnailUrl, CancellationToken ct = default)
    {
        var video = await _context.LoadAsync<Video>(videoId, ct);
        if (video == null) return;

        video.ThumbnailUrl = thumbnailUrl;
        await _context.SaveAsync(video, ct);
    }

    public Task<Video?> GetByIdAsync(string videoId, CancellationToken ct = default)
            => _context.LoadAsync<Video>(videoId, ct);

    public Task SaveAsync(Video video, CancellationToken ct = default)
        => _context.SaveAsync(video, ct);

    public Task DeleteAsync(string videoId)
        => _context.DeleteAsync<Video>(videoId);

    public async Task<int> IncreaseViewCountAsync(string videoId)
    {
        var request = new UpdateItemRequest
        {
            TableName = "Videos",
            Key = new Dictionary<string, AttributeValue>
            {
                ["videoId"] = new AttributeValue { S = videoId }
            },
            UpdateExpression = "SET viewCount = if_not_exists(viewCount, :zero) + :inc",
            ExpressionAttributeValues = new Dictionary<string, AttributeValue>
            {
                [":zero"] = new AttributeValue { N = "0" },
                [":inc"] = new AttributeValue { N = "1" }
            },
            ReturnValues = "UPDATED_NEW"
        };

        var response = await _client.UpdateItemAsync(request);

        return int.Parse(response.Attributes["viewCount"].N);
    }
    public async Task<List<Video>> GetTrendingAsync(int limit = 20)
    {
        // Scan toàn bộ bảng Videos
        var items = await _context.ScanAsync<Video>(new List<ScanCondition>()).GetRemainingAsync();

        // Sort theo viewCount DESC và lấy limit
        return items
            .OrderByDescending(v => v.ViewCount)
            .Take(limit)
            .ToList();
    }
}
