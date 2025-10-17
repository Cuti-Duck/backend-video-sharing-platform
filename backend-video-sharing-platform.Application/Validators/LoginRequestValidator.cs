using FluentValidation;
using backend_video_sharing_platform.Application.DTOs.Auth;

namespace backend_video_sharing_platform.Application.Validators
{
    public class LoginRequestValidator : AbstractValidator<LoginRequest>
    {
        public LoginRequestValidator()
        {
            RuleFor(x => x.Email).NotEmpty().EmailAddress();
            RuleFor(x => x.Password).NotEmpty().MinimumLength(8);
        }
    }
}
