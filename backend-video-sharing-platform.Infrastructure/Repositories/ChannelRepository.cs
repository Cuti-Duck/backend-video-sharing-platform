using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2.DocumentModel;
using backend_video_sharing_platform.Domain.Entities;
using backend_video_sharing_platform.Application.Interfaces;

namespace backend_video_sharing_platform.Infrastructure.Repositories
{
    public class ChannelRepository : IChannelRepository
    {
        private readonly IDynamoDBContext _db;

        public ChannelRepository(IDynamoDBContext db)
        {
            _db = db;
        }

        public async Task<Channel?> GetByUserIdAsync(string userId)
        {
            var conditions = new List<ScanCondition>
            {
                // Quan trọng: dùng nameof(Channel.UserId)
                new ScanCondition(nameof(Channel.UserId), ScanOperator.Equal, userId)
            };

            var result = await _db.ScanAsync<Channel>(conditions).GetRemainingAsync();
            return result.FirstOrDefault();
        }

        public async Task SaveAsync(Channel channel)
        {
            await _db.SaveAsync(channel);
        }

        public Task<Channel?> GetByIdAsync(string channelId)
        {
            return _db.LoadAsync<Channel>(channelId);  // LOAD BY HASH KEY
        }

    }
}
