using backend_video_sharing_platform.Application.Common.Exceptions;
using backend_video_sharing_platform.Application.DTOs.Comment;
using backend_video_sharing_platform.Application.Interfaces;
using backend_video_sharing_platform.Domain.Entities;

namespace backend_video_sharing_platform.Application.Services
{
    public class CommentLikeService : ICommentLikeService
    {
        private readonly ICommentLikeRepository _likeRepo;
        private readonly ICommentRepository _commentRepo;

        public CommentLikeService(
            ICommentLikeRepository likeRepo,
            ICommentRepository commentRepo)
        {
            _likeRepo = likeRepo;
            _commentRepo = commentRepo;
        }

        public async Task<LikeCommentResponse> ToggleLikeAsync(string videoId, string commentId, string userId)
        {
            var comment = await _commentRepo.GetAsync(videoId, commentId);

            if (comment == null)
                throw new NotFoundException("Comment not found.");

            var existing = await _likeRepo.GetAsync(commentId, userId);

            if (existing == null)
            {
                // LIKE
                await _likeRepo.SaveAsync(new CommentLike
                {
                    CommentId = commentId,
                    UserId = userId,
                    CreatedAt = DateTime.UtcNow.ToString("o")
                });

                comment.LikeCount++;
                await _commentRepo.UpdateAsync(comment);

                return new LikeCommentResponse
                {
                    CommentId = commentId,
                    Liked = true,
                    LikeCount = comment.LikeCount
                };
            }
            else
            {
                // UNLIKE
                await _likeRepo.DeleteAsync(commentId, userId);

                if (comment.LikeCount > 0)
                    comment.LikeCount--;

                await _commentRepo.UpdateAsync(comment);

                return new LikeCommentResponse
                {
                    CommentId = commentId,
                    Liked = false,
                    LikeCount = comment.LikeCount
                };
            }
        }
        public async Task<CommentLikeCountResponse> GetLikeCountAsync(string commentId)
        {
            var count = await _likeRepo.CountAsync(commentId);

            return new CommentLikeCountResponse
            {
                CommentId = commentId,
                LikeCount = count
            };
        }

        public async Task<CommentLikeStatusResponse> CheckLikedAsync(string commentId, string userId)
        {
            var existing = await _likeRepo.GetAsync(commentId, userId);

            return new CommentLikeStatusResponse
            {
                CommentId = commentId,
                Liked = existing != null
            };
        }


    }
}
