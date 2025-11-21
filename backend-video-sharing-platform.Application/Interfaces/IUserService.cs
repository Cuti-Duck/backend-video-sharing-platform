using backend_video_sharing_platform.Application.DTOs.User;

namespace backend_video_sharing_platform.Application.Interfaces
{
    public interface IUserService
    {
        Task<bool> UpdateUserAsync(string userId, UpdateUserRequest request);
        Task<UploadAvatarResponse?> UploadAvatarAsync(string userId, Stream fileStream, string fileName, string contentType, CancellationToken ct = default);

        Task<IEnumerable<UserResponse>> GetAllUsersAsync();

        Task<UserResponse> GetUserByIdAsync(string userId);
    }
}
