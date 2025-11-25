using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2.DocumentModel;
using backend_video_sharing_platform.Application.Interfaces;
using backend_video_sharing_platform.Domain.Entities;

namespace backend_video_sharing_platform.Infrastructure.Repositories
{
    public class SubscriptionRepository : ISubscriptionRepository
    {
        private readonly IDynamoDBContext _db;

        public SubscriptionRepository(IDynamoDBContext db)
        {
            _db = db;
        }

        public Task<Subscription?> GetAsync(string userId, string channelId)
            => _db.LoadAsync<Subscription>(userId, channelId);

        public Task SaveAsync(Subscription subscription)
            => _db.SaveAsync(subscription);

        public async Task<int> CountSubscribersAsync(string channelId)
        {
            var conditions = new List<ScanCondition>
            {
                new ScanCondition(nameof(Subscription.ChannelId), ScanOperator.Equal, channelId)
            };

            var items = await _db.ScanAsync<Subscription>(conditions).GetRemainingAsync();
            return items.Count;
        }
        public Task DeleteAsync(string userId, string channelId)
            => _db.DeleteAsync<Subscription>(userId, channelId);

        public async Task<List<Subscription>> GetUserSubscriptionsAsync(string userId)
        {
            var queryConfig = new DynamoDBOperationConfig
            {
                QueryFilter = new List<ScanCondition>()
            };

            var items = await _db.QueryAsync<Subscription>(userId, queryConfig).GetRemainingAsync();
            return items;
        }

        public async Task<List<Subscription>> GetChannelSubscribersAsync(string channelId)
        {
            var conditions = new List<ScanCondition>
            {
                new ScanCondition(nameof(Subscription.ChannelId), ScanOperator.Equal, channelId)
            };

            var items = await _db.ScanAsync<Subscription>(conditions).GetRemainingAsync();
            return items;
        }
    }
}
