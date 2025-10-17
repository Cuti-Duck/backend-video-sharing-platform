using backend_video_sharing_platform.Application.DTOs.Auth;

namespace backend_video_sharing_platform.Application.Interfaces
{
    public interface ICognitoAuthService
    {
        Task<RegisterUserResponse> RegisterAsync(RegisterUserRequest request, CancellationToken ct = default);
        Task<BasicResponse> ConfirmSignUpAsync(ConfirmSignUpRequest request, CancellationToken ct = default);
        Task<BasicResponse> ResendConfirmationCodeAsync(string email, CancellationToken ct = default);
        Task<LoginResponse> LoginAsync(LoginRequest request, CancellationToken ct = default);
    }
}
