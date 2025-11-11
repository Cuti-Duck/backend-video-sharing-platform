namespace backend_video_sharing_platform.Application.DTOs.User
{
    public class UserResponse
    {
        public string UserId { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Gender { get; set; } = string.Empty;
        public string BirthDate { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public string? AvatarUrl { get; set; }
        public string? ChannelId { get; set; }
        public string CreatedAt { get; set; } = string.Empty;
    }
}
