namespace backend_video_sharing_platform.Application.DTOs.Auth
{
    public class RegisterUserRequest
    {
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Gender { get; set; } = string.Empty;
        public string BirthDate { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
    }
}
