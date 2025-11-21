using backend_video_sharing_platform.Domain.Entities;

namespace backend_video_sharing_platform.Application.Interfaces
{
    public interface IUserRepository
    {
        Task<User?> GetByIdAsync(string userId, CancellationToken ct = default);
        Task SaveAsync(User user, CancellationToken ct = default);

        Task<IEnumerable<User>> GetAllUsersAsync(CancellationToken ct = default);


    }
}
