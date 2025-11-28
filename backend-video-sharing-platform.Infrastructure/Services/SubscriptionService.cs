using backend_video_sharing_platform.Application.Common.Exceptions;
using backend_video_sharing_platform.Application.DTOs.Subscription;
using backend_video_sharing_platform.Application.Interfaces;
using backend_video_sharing_platform.Domain.Entities;
using Microsoft.Extensions.Logging;

namespace backend_video_sharing_platform.Application.Services
{
    public class SubscriptionService : ISubscriptionService
    {
        private readonly ISubscriptionRepository _subRepo;
        private readonly IChannelRepository _channelRepo;
        private readonly ILogger<SubscriptionService> _logger;
        private readonly IUserRepository _userRepo;

        public SubscriptionService(
            ISubscriptionRepository subRepo,
            IChannelRepository channelRepo,
            ILogger<SubscriptionService> logger,
            IUserRepository userRepo)
        {
            _subRepo = subRepo;
            _channelRepo = channelRepo;
            _logger = logger;
            _userRepo = userRepo;
        }

        public async Task<ChannelSubscribersResponse> GetChannelSubscribersAsync(string channelId)
        {
            var channel = await _channelRepo.GetByIdAsync(channelId);
            if (channel == null)
                throw new NotFoundException("Channel does not exist.");

            var subscriptions = await _subRepo.GetChannelSubscribersAsync(channelId);

            var subscribers = new List<SubscriberDto>();

            foreach (var sub in subscriptions)
            {
                var user = await _userRepo.GetByIdAsync(sub.UserId);
                if (user != null)
                {
                    subscribers.Add(new SubscriberDto
                    {
                        ChannelId = user.UserId,
                        ChannelName = user.Name,
                        AvatarUrl = user.AvatarUrl,
                        SubscribedAt = sub.CreatedAt
                    });
                }
            }

            return new ChannelSubscribersResponse
            {
                ChannelId = channelId,
                Subscribers = subscribers.OrderByDescending(s => s.SubscribedAt).ToList(),
                TotalCount = subscribers.Count
            };
        }

        public async Task<MySubscriptionsResponse> GetMySubscriptionsAsync(string userId)
        {
            var subscriptions = await _subRepo.GetUserSubscriptionsAsync(userId);

            var channels = new List<SubscribedChannelDto>();

            foreach (var sub in subscriptions)
            {
                var channel = await _channelRepo.GetByIdAsync(sub.ChannelId);
                if (channel != null)
                {
                    channels.Add(new SubscribedChannelDto
                    {
                        ChannelId = channel.ChannelId,
                        ChannelName = channel.Name,
                        AvatarUrl = channel.AvatarUrl,
                        SubscriberCount = channel.SubscriberCount,
                        SubscribedAt = sub.CreatedAt
                    });
                }
            }

            return new MySubscriptionsResponse
            {
                Channels = channels.OrderByDescending(c => c.SubscribedAt).ToList(),
                TotalCount = channels.Count
            };
        }

        public async Task<SubscribeResponse> SubscribeAsync(string userId, SubscribeRequest request)
        {
            var channel = await _channelRepo.GetByIdAsync(request.ChannelId);
            if (channel == null)
                throw new NotFoundException("Channel does not exist.");

            if (channel.UserId == userId)
                throw new BadRequestException("You cannot subscribe to your own channel.");

            var existing = await _subRepo.GetAsync(userId, request.ChannelId);
            if (existing != null)
                throw new BadRequestException("You already subscribed this channel.");

            var subscription = new Subscription
            {
                UserId = userId,
                ChannelId = request.ChannelId,
                CreatedAt = DateTime.UtcNow.ToString("o")
            };

            await _subRepo.SaveAsync(subscription);

            channel.SubscriberCount++;
            await _channelRepo.SaveAsync(channel);

            return new SubscribeResponse
            {
                ChannelId = request.ChannelId,
                Success = true,
                SubscriberCount = channel.SubscriberCount
            };
        }
        public async Task<UnsubscribeResponse> UnsubscribeAsync(string userId, UnsubscribeRequest request)
        {
            var channel = await _channelRepo.GetByIdAsync(request.ChannelId);
            if (channel == null)
                throw new NotFoundException("Channel does not exist.");
            var existing = await _subRepo.GetAsync(userId, request.ChannelId);
            if (existing == null)
                throw new BadRequestException("You are not subscribed to this channel.");
            await _subRepo.DeleteAsync(userId, request.ChannelId);
            if (channel.SubscriberCount > 0)
            {
                channel.SubscriberCount--;
                await _channelRepo.SaveAsync(channel);
            }
            return new UnsubscribeResponse
            {
                ChannelId = request.ChannelId,
                Success = true,
                SubscriberCount = channel.SubscriberCount
            };
        }
    }
}
