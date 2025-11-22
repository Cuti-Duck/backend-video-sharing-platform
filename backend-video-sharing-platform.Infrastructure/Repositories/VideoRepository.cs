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
}
