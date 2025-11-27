using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2.DocumentModel;
using backend_video_sharing_platform.Application.Interfaces;
using backend_video_sharing_platform.Domain.Entities;

namespace backend_video_sharing_platform.Infrastructure.Repositories
{
    public class CommentRepository : ICommentRepository
    {
        private readonly IDynamoDBContext _db;

        public CommentRepository(IDynamoDBContext db)
        {
            _db = db;
        }

        public Task<Comment?> GetAsync(string videoId, string commentId)
            => _db.LoadAsync<Comment>(videoId, commentId);

        public Task SaveAsync(Comment comment)
            => _db.SaveAsync(comment);

        public Task UpdateAsync(Comment comment)
            => _db.SaveAsync(comment);

        public async Task<List<Comment>> GetVideoCommentsAsync(string videoId)
        {
            var items = await _db.QueryAsync<Comment>(videoId).GetRemainingAsync();
            return items.Where(c => !c.IsDeleted).ToList();
        }

        public async Task<List<Comment>> GetCommentRepliesAsync(string videoId, string parentCommentId)
        {
            var allComments = await GetVideoCommentsAsync(videoId);
            return allComments
                .Where(c => c.ParentCommentId == parentCommentId)
                .OrderBy(c => c.CreatedAt)
                .ToList();
        }

        public async Task<int> CountVideoCommentsAsync(string videoId)
        {
            var items = await _db.QueryAsync<Comment>(videoId).GetRemainingAsync();
            return items.Count(c => !c.IsDeleted);
        }

        public Task DeleteAsync(string videoId, string commentId)
            => _db.DeleteAsync<Comment>(videoId, commentId);

        public async Task<List<Comment>> GetAllCommentsIncludingDeleted(string videoId)
        {
            var items = await _db.QueryAsync<Comment>(videoId).GetRemainingAsync();
            return items;
        }
    }
}