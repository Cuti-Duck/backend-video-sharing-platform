using backend_video_sharing_platform.Application.Common.Exceptions;
using backend_video_sharing_platform.Application.DTOs.Comment;
using backend_video_sharing_platform.Application.Interfaces;
using backend_video_sharing_platform.Domain.Entities;
using Microsoft.Extensions.Logging;

namespace backend_video_sharing_platform.Application.Services
{
    public class CommentService : ICommentService
    {
        private readonly ICommentRepository _commentRepo;
        private readonly IVideoRepository _videoRepo;
        private readonly IUserRepository _userRepo;
        private readonly ILogger<CommentService> _logger;

        public CommentService(
            ICommentRepository commentRepo,
            IVideoRepository videoRepo,
            IUserRepository userRepo,
            ILogger<CommentService> logger)
        {
            _commentRepo = commentRepo;
            _videoRepo = videoRepo;
            _userRepo = userRepo;
            _logger = logger;
        }

        public async Task<CreateCommentResponse> CreateCommentAsync(
            string videoId,
            string userId,
            CreateCommentRequest request)
        {
            // Validate content
            if (string.IsNullOrWhiteSpace(request.Content))
                throw new BadRequestException("Comment content cannot be empty.");

            if (request.Content.Length > 10000)
                throw new BadRequestException("Comment content is too long (max 10000 characters).");

            var video = await _videoRepo.GetByIdAsync(videoId);
            if (video == null)
                throw new NotFoundException("Video not found.");

            var user = await _userRepo.GetByIdAsync(userId);
            if (user == null)
                throw new NotFoundException("User not found.");

            var parentCommentId = string.IsNullOrWhiteSpace(request.ParentCommentId)
                ? null
                : request.ParentCommentId.Trim();

            Comment? parentComment = null;
            if (!string.IsNullOrEmpty(parentCommentId))
            {
                parentComment = await _commentRepo.GetAsync(videoId, parentCommentId);
                if (parentComment == null)
                    throw new NotFoundException("Parent comment not found.");

                if (parentComment.IsDeleted)
                    throw new BadRequestException("Cannot reply to a deleted comment.");
            }

            var comment = new Comment
            {
                VideoId = videoId,
                CommentId = Guid.NewGuid().ToString(),
                UserId = userId,
                UserName = user.Name,
                UserAvatarUrl = user.AvatarUrl,
                Content = request.Content.Trim(),
                ParentCommentId = parentCommentId,
                LikeCount = 0,
                ReplyCount = 0,
                IsEdited = false,
                IsDeleted = false,
                CreatedAt = DateTime.UtcNow.ToString("o"),
                UpdatedAt = null
            };

            await _commentRepo.SaveAsync(comment);

            if (parentComment != null)
            {
                parentComment.ReplyCount++;
                await _commentRepo.SaveAsync(parentComment);
            }

            video.CommentCount++;
            await _videoRepo.SaveAsync(video);

            _logger.LogInformation(
                "User {UserId} created comment {CommentId} on video {VideoId}",
                userId, comment.CommentId, videoId
            );

            return new CreateCommentResponse
            {
                CommentId = comment.CommentId,
                VideoId = comment.VideoId,
                UserId = comment.UserId,
                UserName = comment.UserName,
                UserAvatarUrl = comment.UserAvatarUrl,
                Content = comment.Content,
                ParentCommentId = comment.ParentCommentId,
                LikeCount = comment.LikeCount,
                ReplyCount = comment.ReplyCount,
                CreatedAt = comment.CreatedAt,
                Message = parentComment != null
                    ? "Reply posted successfully"
                    : "Comment posted successfully"
            };
        }

        public async Task<DeleteCommentResponse> DeleteCommentAsync(
            string videoId,
            string commentId,
            string userId)
        {
            // 1. Get existing comment
            var comment = await _commentRepo.GetAsync(videoId, commentId);
            if (comment == null)
                throw new NotFoundException("Comment not found.");

            // 2. Get video
            var video = await _videoRepo.GetByIdAsync(videoId);
            if (video == null)
                throw new NotFoundException("Video not found.");

            // 3. Check ownership - only owner or video owner can delete
            bool isCommentOwner = comment.UserId == userId;
            bool isVideoOwner = video.UserId == userId;

            if (!isCommentOwner && !isVideoOwner)
                throw new ForbiddenException("You can only delete your own comments or comments on your videos.");

            // 4. Get all child comments recursively (CASCADE DELETE)
            var allCommentsToDelete = await GetCommentTreeAsync(videoId, commentId);
            int totalDeleted = allCommentsToDelete.Count;

            // 5. Delete all comments from DB (parent + all children)
            foreach (var commentToDelete in allCommentsToDelete)
            {
                await _commentRepo.DeleteAsync(videoId, commentToDelete.CommentId);
            }

            // 6. Update parent comment's reply count if this was a reply
            if (!string.IsNullOrEmpty(comment.ParentCommentId))
            {
                var parentComment = await _commentRepo.GetAsync(videoId, comment.ParentCommentId);
                if (parentComment != null && parentComment.ReplyCount > 0)
                {
                    parentComment.ReplyCount--;
                    await _commentRepo.UpdateAsync(parentComment);
                }
            }

            // 7. Update video's comment count
            if (video.CommentCount >= totalDeleted)
            {
                video.CommentCount -= totalDeleted;
                await _videoRepo.SaveAsync(video);
            }

            _logger.LogInformation(
                "User {UserId} deleted comment {CommentId} and {ChildCount} child comments on video {VideoId}",
                userId, commentId, totalDeleted - 1, videoId
            );

            return new DeleteCommentResponse
            {
                CommentId = commentId,
                VideoId = videoId,
                DeletedCount = totalDeleted,
                Success = true,
                Message = totalDeleted > 1
                    ? $"Comment and {totalDeleted - 1} replies deleted successfully"
                    : "Comment deleted successfully"
            };
        }

        public async Task<GetCommentsResponse> GetCommentsAsync(
            string videoId,
            GetCommentsRequest request)
        {
            var video = await _videoRepo.GetByIdAsync(videoId);
            if (video == null)
                throw new NotFoundException("Video not found.");

            var allComments = await _commentRepo.GetVideoCommentsAsync(videoId);
            var rootComments = allComments.Where(c => string.IsNullOrEmpty(c.ParentCommentId)).ToList();
            var replies = allComments.Where(c => !string.IsNullOrEmpty(c.ParentCommentId)).ToList();

            if (!string.IsNullOrEmpty(request.ParentCommentId))
            {
                var parentReplies = await _commentRepo.GetCommentRepliesAsync(videoId, request.ParentCommentId);
                var replyDtos = parentReplies.Select(c => MapToCommentDto(c)).ToList();

                return new GetCommentsResponse
                {
                    VideoId = videoId,
                    Comments = replyDtos,
                    TotalCount = replyDtos.Count,
                    RootCommentsCount = 0,
                    RepliesCount = replyDtos.Count
                };
            }

            var sortedRootComments = request.SortBy.ToLower() switch
            {
                "popular" => rootComments.OrderByDescending(c => c.LikeCount).ThenByDescending(c => c.CreatedAt),
                "oldest" => rootComments.OrderBy(c => c.CreatedAt),
                _ => rootComments.OrderByDescending(c => c.CreatedAt)
            };

            var paginatedComments = sortedRootComments
                .Skip(request.Offset)
                .Take(request.Limit)
                .ToList();

            var commentDtos = new List<CommentDto>();
            foreach (var comment in paginatedComments)
            {
                var dto = MapToCommentDto(comment);

                if (request.IncludeReplies && comment.ReplyCount > 0)
                {
                    var commentReplies = replies
                        .Where(r => r.ParentCommentId == comment.CommentId)
                        .OrderBy(r => r.CreatedAt)
                        .Take(20)
                        .Select(r => MapToCommentDto(r))
                        .ToList();

                    dto.Replies = commentReplies;
                }

                commentDtos.Add(dto);
            }

            return new GetCommentsResponse
            {
                VideoId = videoId,
                Comments = commentDtos,
                TotalCount = allComments.Count,
                RootCommentsCount = rootComments.Count,
                RepliesCount = replies.Count
            };
        }

        public async Task<UpdateCommentResponse> UpdateCommentAsync(
            string videoId,
            string commentId,
            string userId,
            UpdateCommentRequest request)
        {
            // 1. Validate content
            if (string.IsNullOrWhiteSpace(request.Content))
                throw new BadRequestException("Comment content cannot be empty.");

            if (request.Content.Length > 10000)
                throw new BadRequestException("Comment content is too long (max 10000 characters).");

            // 2. Get existing comment
            var comment = await _commentRepo.GetAsync(videoId, commentId);
            if (comment == null)
                throw new NotFoundException("Comment not found.");

            // 3. Check if comment is deleted
            if (comment.IsDeleted)
                throw new BadRequestException("Cannot edit a deleted comment.");

            // 4. Check ownership
            if (comment.UserId != userId)
                throw new ForbiddenException("You can only edit your own comments.");

            // 5. Check if content changed
            var newContent = request.Content.Trim();
            if (comment.Content == newContent)
                throw new BadRequestException("New content is the same as current content.");

            // 6. Update comment
            comment.Content = newContent;
            comment.IsEdited = true;
            comment.UpdatedAt = DateTime.UtcNow.ToString("o");

            await _commentRepo.UpdateAsync(comment);

            _logger.LogInformation(
                "User {UserId} updated comment {CommentId} on video {VideoId}",
                userId, commentId, videoId
            );

            return new UpdateCommentResponse
            {
                CommentId = comment.CommentId,
                VideoId = comment.VideoId,
                Content = comment.Content,
                IsEdited = comment.IsEdited,
                UpdatedAt = comment.UpdatedAt!,
                Message = "Comment updated successfully"
            };
        }

        private CommentDto MapToCommentDto(Comment comment)
        {
            return new CommentDto
            {
                CommentId = comment.CommentId,
                VideoId = comment.VideoId,
                UserId = comment.UserId,
                UserName = comment.UserName,
                UserAvatarUrl = comment.UserAvatarUrl,
                Content = comment.Content,
                ParentCommentId = comment.ParentCommentId,
                LikeCount = comment.LikeCount,
                ReplyCount = comment.ReplyCount,
                IsEdited = comment.IsEdited,
                CreatedAt = comment.CreatedAt,
                UpdatedAt = comment.UpdatedAt,
                Replies = null
            };
        }

        private async Task<List<Comment>> GetCommentTreeAsync(string videoId, string commentId)
        {
            var result = new List<Comment>();

            // Get the comment itself
            var comment = await _commentRepo.GetAsync(videoId, commentId);
            if (comment != null)
            {
                result.Add(comment);

                // Get direct children
                var directChildren = await _commentRepo.GetCommentRepliesAsync(videoId, commentId);

                // Recursively get children of children
                foreach (var child in directChildren)
                {
                    var childTree = await GetCommentTreeAsync(videoId, child.CommentId);
                    result.AddRange(childTree);
                }
            }

            return result;
        }
    }
}
