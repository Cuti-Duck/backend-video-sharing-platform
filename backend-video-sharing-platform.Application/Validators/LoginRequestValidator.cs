using FluentValidation;
using backend_video_sharing_platform.Application.DTOs.Auth;

namespace backend_video_sharing_platform.Application.Validators
{
    public class LoginRequestValidator : AbstractValidator<LoginRequest>
    {
        public LoginRequestValidator()
        {
            RuleFor(x => x)
                .Must(x => IsValidCredentials(x.Email, x.Password))
                .WithMessage("Email or password is incorrect");
        }

        private bool IsValidCredentials(string email, string password)
        {
            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
                return false;

            
            if (!email.Contains("@"))
                return false;

            
            return password.Length >= 8;
        }
    }
}
