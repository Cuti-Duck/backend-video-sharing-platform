using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2.DocumentModel;
using backend_video_sharing_platform.Application.Interfaces;
using backend_video_sharing_platform.Domain.Entities;

namespace backend_video_sharing_platform.Infrastructure.Repositories
{
    public class VideoLikeRepository : IVideoLikeRepository
    {
        private readonly IDynamoDBContext _db;

        public VideoLikeRepository(IDynamoDBContext db)
        {
            _db = db;
        }

        public Task<VideoLike?> GetAsync(string userId, string videoId)
            => _db.LoadAsync<VideoLike>(userId, videoId);

        public Task SaveAsync(VideoLike like)
            => _db.SaveAsync(like);

        public Task DeleteAsync(string userId, string videoId)
            => _db.DeleteAsync<VideoLike>(userId, videoId);

        public async Task<List<VideoLike>> GetUserLikesAsync(string userId)
        {
            var items = await _db.QueryAsync<VideoLike>(userId).GetRemainingAsync();
            return items;
        }

        public async Task<int> CountVideoLikesAsync(string videoId)
        {
            var conditions = new List<ScanCondition>
            {
                new ScanCondition(nameof(VideoLike.VideoId), ScanOperator.Equal, videoId)
            };
            var items = await _db.ScanAsync<VideoLike>(conditions).GetRemainingAsync();
            return items.Count;
        }
    }
}
