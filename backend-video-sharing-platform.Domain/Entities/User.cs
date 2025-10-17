namespace backend_video_sharing_platform.Domain.Entities
{
    public class User
    {
        public string Sub { get; set; } = string.Empty;       // Cognito 'sub'
        public string Email { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Gender { get; set; } = string.Empty;     // male/female/unspecified
        public string BirthDate { get; set; } = string.Empty;  // yyyy-MM-dd
        public string PhoneNumber { get; set; } = string.Empty;// +84...
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
