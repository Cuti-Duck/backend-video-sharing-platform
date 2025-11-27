using backend_video_sharing_platform.Application.Common.Exceptions;
using backend_video_sharing_platform.Application.DTOs.Video;
using backend_video_sharing_platform.Application.DTOs.VideoLike;
using backend_video_sharing_platform.Application.Interfaces;
using backend_video_sharing_platform.Domain.Entities;
using Microsoft.Extensions.Logging;

namespace backend_video_sharing_platform.Application.Services
{
    public class VideoLikeService : IVideoLikeService
    {
        private readonly IVideoLikeRepository _likeRepo;
        private readonly IVideoRepository _videoRepo;
        private readonly IChannelRepository _channelRepo;
        private readonly ILogger<VideoLikeService> _logger;

        public VideoLikeService(
            IVideoLikeRepository likeRepo,
            IVideoRepository videoRepo,
            IChannelRepository channelRepo,
            ILogger<VideoLikeService> logger)
        {
            _likeRepo = likeRepo;
            _videoRepo = videoRepo;
            _channelRepo = channelRepo;
            _logger = logger;
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
            var videos = new List<LikedVideoDto>();

            foreach (var like in likes)
            {
                var video = await _videoRepo.GetByIdAsync(like.VideoId);
                if (video != null)
                {
                    var channel = await _channelRepo.GetByIdAsync(video.ChannelId);

                    videos.Add(new LikedVideoDto
                    {
                        VideoId = video.VideoId,
                        Title = video.Title,
                        ThumbnailUrl = video.ThumbnailUrl,
                        LikedAt = like.CreatedAt,
                        LikeCount = (int)video.LikeCount,
                        ViewCount = (int)video.ViewCount,
                        ChannelName = channel?.Name ?? "Unknown"
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
