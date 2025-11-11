using Amazon.DynamoDBv2.DataModel;
using backend_video_sharing_platform.Application.Interfaces;
using backend_video_sharing_platform.Domain.Entities;

namespace backend_video_sharing_platform.Infrastructure.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly IDynamoDBContext _db;

        public UserRepository(IDynamoDBContext db)
        {
            _db = db;
        }

        public Task<User?> GetByIdAsync(string userId, CancellationToken ct = default)
            => _db.LoadAsync<User>(userId, ct);

        public Task SaveAsync(User user, CancellationToken ct = default)
            => _db.SaveAsync(user, ct);
    }
}
