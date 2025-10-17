namespace backend_video_sharing_platform.Application.DTOs.Auth
{
    public class LoginResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public string? AccessToken { get; set; }
        public string? IdToken { get; set; }
        public string? RefreshToken { get; set; }
    }
}
