using Amazon.DynamoDBv2.DataModel;
using backend_video_sharing_platform.Application.Interfaces;
using backend_video_sharing_platform.Domain.Entities;

namespace backend_video_sharing_platform.Infrastructure.Repositories
{
    public class CommentLikeRepository : ICommentLikeRepository
    {
        private readonly IDynamoDBContext _db;

        public CommentLikeRepository(IDynamoDBContext db)
        {
            _db = db;
        }

        public Task<CommentLike?> GetAsync(string commentId, string userId)
            => _db.LoadAsync<CommentLike>(commentId, userId);

        public Task SaveAsync(CommentLike like)
            => _db.SaveAsync(like);

        public Task DeleteAsync(string commentId, string userId)
            => _db.DeleteAsync<CommentLike>(commentId, userId);

        public async Task<int> CountAsync(string commentId)
        {
            var items = await _db.QueryAsync<CommentLike>(commentId).GetRemainingAsync();
            return items.Count;
        }
    }
}
