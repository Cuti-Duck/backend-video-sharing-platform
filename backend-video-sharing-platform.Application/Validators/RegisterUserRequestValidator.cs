using FluentValidation;
using backend_video_sharing_platform.Application.DTOs.Auth;
using System.Globalization;
using System.Text.RegularExpressions;

namespace backend_video_sharing_platform.Application.Validators
{
    public class RegisterUserRequestValidator : AbstractValidator<RegisterUserRequest>
    {
        public RegisterUserRequestValidator()
        {
            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("Email is required.")
                .EmailAddress().WithMessage("Invalid email format.");

            RuleFor(x => x.Password)
                .NotEmpty().MinimumLength(8)
                .Matches("[A-Z]").WithMessage("Password must contain at least one uppercase letter.")
                .Matches("[a-z]").WithMessage("Password must contain at least one lowercase letter.")
                .Matches("[0-9]").WithMessage("Password must contain at least one number.")
                .Matches("[^a-zA-Z0-9]").WithMessage("Password must contain at least one special character.");

            RuleFor(x => x.Name).NotEmpty().MaximumLength(100);

            RuleFor(x => x.BirthDate)
                .NotEmpty()
                .Must(d => DateTime.TryParseExact(d, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out _))
                .WithMessage("BirthDate must be yyyy-MM-dd.");

            RuleFor(x => x.PhoneNumber)
                .Must(p => string.IsNullOrWhiteSpace(p) || Regex.IsMatch(p, @"^\+?\d{9,15}$"))
                .WithMessage("PhoneNumber must be E.164 format (e.g. +84901234567).");
        }
    }
}
