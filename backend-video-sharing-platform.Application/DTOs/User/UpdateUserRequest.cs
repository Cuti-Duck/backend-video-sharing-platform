namespace backend_video_sharing_platform.Application.DTOs.User
{
    public class UpdateUserRequest
    {
        public string? Name { get; set; }
        public string? Gender { get; set; }
        public string? BirthDate { get; set; }
        public string? PhoneNumber { get; set; }
    }
}
