namespace backend_video_sharing_platform.Application.DTOs.Auth
{
    public class ConfirmSignUpRequest
    {
        public string Email { get; set; } = string.Empty;
        public string Code { get; set; } = string.Empty;
    }
}
