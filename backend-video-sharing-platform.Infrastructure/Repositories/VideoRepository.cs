using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using backend_video_sharing_platform.Application.Interfaces;
using backend_video_sharing_platform.Domain.Entities;

public class VideoRepository : IVideoRepository
{
    private readonly IDynamoDBContext _context;

    public VideoRepository(IDynamoDBContext context)
    {
        _context = context;
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
}
