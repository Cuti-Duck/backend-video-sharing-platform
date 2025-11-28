using backend_video_sharing_platform.Application.Common.Exceptions;
using backend_video_sharing_platform.Application.DTOs.Notification;
using backend_video_sharing_platform.Application.DTOs.Video;
using backend_video_sharing_platform.Application.DTOs.VideoLike;
using backend_video_sharing_platform.Application.Interfaces;
using backend_video_sharing_platform.Domain.Entities;
using backend_video_sharing_platform.Domain.Enums;
using Microsoft.Extensions.Logging;

namespace backend_video_sharing_platform.Application.Services
{
    public class VideoLikeService : IVideoLikeService
    {
        private readonly IVideoLikeRepository _likeRepo;
        private readonly IVideoRepository _videoRepo;
        private readonly IChannelRepository _channelRepo;
        private readonly INotificationService _notificationService; 
        private readonly ILogger<VideoLikeService> _logger;
        private readonly IUserRepository _userRepo;

        public VideoLikeService(
            IVideoLikeRepository likeRepo,
            IVideoRepository videoRepo,
            IChannelRepository channelRepo,
            INotificationService notificationService,
            IUserRepository userRepo,
            ILogger<VideoLikeService> logger)
        {
            _likeRepo = likeRepo;
            _videoRepo = videoRepo;
            _channelRepo = channelRepo;
            _notificationService = notificationService; 
            _logger = logger;
            _userRepo = userRepo;
        }

        public async Task<ToggleLikeResponse> ToggleLikeAsync(string userId, string videoId)
        {
            // Check if video exists
            var video = await _videoRepo.GetByIdAsync(videoId);
            if (video == null)
                throw new NotFoundException("Video not found.");

            // Check current like status
            var existingLike = await _likeRepo.GetAsync(userId, videoId);

            if (existingLike != null)
            {
                // Unlike: Remove the like
                await _likeRepo.DeleteAsync(userId, videoId);

                // Decrease like count
                if (video.LikeCount > 0)
                    video.LikeCount--;

                await _videoRepo.SaveAsync(video);

                return new ToggleLikeResponse
                {
                    VideoId = videoId,
                    IsLiked = false,
                    TotalLikes = (int)video.LikeCount,
                    Message = "Video unliked successfully"
                };
            }
            else
            {
                // Like: Add new like
                var newLike = new VideoLike
                {
                    UserId = userId,
                    VideoId = videoId,
                    CreatedAt = DateTime.UtcNow.ToString("o")
                };

                await _likeRepo.SaveAsync(newLike);

                // Increase like count
                video.LikeCount++;
                await _videoRepo.SaveAsync(video);

                // ✅ NEW: Create notification (nếu không phải tự like video của mình)
                if (video.UserId != userId)
                {
                    try
                    {
                        await _notificationService.CreateNotificationAsync(new CreateNotificationRequest
                        {
                            RecipientUserId = video.UserId,
                            ActorUserId = userId,
                            Type = nameof(NotificationType.VIDEO_LIKED),
                            VideoId = videoId,
                            CommentId = null
                        });

                        _logger.LogInformation(
                            "Created VIDEO_LIKED notification for user {RecipientId} from user {ActorId}",
                            video.UserId,
                            userId
                        );
                    }
                    catch (Exception ex)
                    {
                        // Notification failure should not break the like operation
                        _logger.LogError(ex, "Failed to create notification for video like");
                    }
                }

                return new ToggleLikeResponse
                {
                    VideoId = videoId,
                    IsLiked = true,
                    TotalLikes = (int)video.LikeCount,
                    Message = "Video liked successfully"
                };
            }
        }

        public async Task<VideoLikeStatusResponse> GetLikeStatusAsync(string userId, string videoId)
        {
            var video = await _videoRepo.GetByIdAsync(videoId);
            if (video == null)
                throw new NotFoundException("Video not found.");

            var like = await _likeRepo.GetAsync(userId, videoId);

            return new VideoLikeStatusResponse
            {
                VideoId = videoId,
                IsLiked = like != null,
                TotalLikes = (int)video.LikeCount
            };
        }

        public async Task<UserLikedVideosResponse> GetUserLikedVideosAsync(string userId)
        {
            var likes = await _likeRepo.GetUserLikesAsync(userId);

            if (likes == null || !likes.Any())
            {
                return new UserLikedVideosResponse
                {
                    Videos = new List<LikedVideoDto>(),
                    TotalCount = 0
                };
            }

            var videos = new List<LikedVideoDto>();

            foreach (var like in likes)
            {
                var video = await _videoRepo.GetByIdAsync(like.VideoId);
                if (video != null)
                {
                    var channel = await _channelRepo.GetByIdAsync(video.ChannelId);

                    // Lấy thông tin user
                    User? user = null;
                    if (!string.IsNullOrEmpty(video.UserId))
                    {
                        user = await _userRepo.GetByIdAsync(video.UserId);
                    }

                    videos.Add(new LikedVideoDto
                    {
                        VideoId = video.VideoId,
                        ChannelId = video.ChannelId,
                        UserId = video.UserId,
                        Title = video.Title,
                        Description = video.Description,
                        PlaybackUrl = video.PlaybackUrl,
                        Key = video.Key,
                        Status = video.Status,
                        Duration = video.Duration,
                        ThumbnailUrl = video.ThumbnailUrl,
                        Type = video.Type,
                        ViewCount = video.ViewCount,
                        LikeCount = video.LikeCount,
                        CommentCount = video.CommentCount,
                        CreatedAt = video.CreatedAt,
                        UpdatedAt = video.UpdatedAt,

                        // Additional fields
                        LikedAt = like.CreatedAt,
                        ChannelName = channel?.Name ?? "Unknown",
                        UserName = user?.Name ?? "",
                        UserAvatarUrl = user?.AvatarUrl ?? ""
                    });
                }
            }

            return new UserLikedVideosResponse
            {
                Videos = videos.OrderByDescending(v => v.LikedAt).ToList(),
                TotalCount = videos.Count
            };
        }
    }
}