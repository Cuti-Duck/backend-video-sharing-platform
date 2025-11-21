using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using backend_video_sharing_platform.Application.Interfaces;
using backend_video_sharing_platform.Domain.Entities;

namespace backend_video_sharing_platform.Infrastructure.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly IDynamoDBContext _db;
        private readonly IAmazonDynamoDB _client;

        public UserRepository(IDynamoDBContext db, IAmazonDynamoDB client)

        {
            _client = client;
            _db = db;
        }

        public async Task<IEnumerable<User>> GetAllUsersAsync(CancellationToken ct = default)
        {
            var conditions = new List<ScanCondition>();
            return await _db.ScanAsync<User>(conditions).GetRemainingAsync(ct);
        }

        public Task<User?> GetByIdAsync(string userId, CancellationToken ct = default)
            => _db.LoadAsync<User>(userId, ct);

        public Task SaveAsync(User user, CancellationToken ct = default)
            => _db.SaveAsync(user, ct);
    }
}
