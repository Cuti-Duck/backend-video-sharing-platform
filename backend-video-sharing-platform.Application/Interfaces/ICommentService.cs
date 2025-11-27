using backend_video_sharing_platform.Application.DTOs.Comment;

namespace backend_video_sharing_platform.Application.Interfaces
{
    public interface ICommentService
    {
        Task<CreateCommentResponse> CreateCommentAsync(
            string videoId,
            string userId,
            CreateCommentRequest request
        );

        Task<GetCommentsResponse> GetCommentsAsync(
            string videoId,
            GetCommentsRequest request
        );

        Task<UpdateCommentResponse> UpdateCommentAsync(
            string videoId,
            string commentId,
            string userId,
            UpdateCommentRequest request
        );

        Task<DeleteCommentResponse> DeleteCommentAsync(
            string videoId,
            string commentId,
            string userId
        );
    }
}