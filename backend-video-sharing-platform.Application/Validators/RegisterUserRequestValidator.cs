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
            // Email — required, valid format
            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("Email is required.")
                .EmailAddress().WithMessage("Invalid email format.");

            // Password — required, strong
            RuleFor(x => x.Password)
                .NotEmpty().WithMessage("Password is required.")
                .MinimumLength(8).WithMessage("Password must be at least 8 characters long.")
                .Matches("[A-Z]").WithMessage("Password must contain at least one uppercase letter.")
                .Matches("[a-z]").WithMessage("Password must contain at least one lowercase letter.")
                .Matches("[0-9]").WithMessage("Password must contain at least one number.")
                .Matches("[^a-zA-Z0-9]").WithMessage("Password must contain at least one special character.");

            // Name — required, not too long
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Name is required.")
                .MaximumLength(50).WithMessage("Name must not exceed 50 characters.");

            // Gender — only male or female
            RuleFor(x => x.Gender)
                .Must(g => string.IsNullOrEmpty(g)
                    || g.Equals("male", StringComparison.OrdinalIgnoreCase)
                    || g.Equals("female", StringComparison.OrdinalIgnoreCase))
                .WithMessage("Gender must be either 'male' or 'female'.");

            // BirthDate — correct format yyyy-MM-dd
            RuleFor(x => x.BirthDate)
                .NotEmpty().WithMessage("Birth date is required.")
                .Must(BeValidDateFormat)
                .WithMessage("Birth date must follow the yyyy-MM-dd format.");

            // PhoneNumber — valid international format (E.164)
            RuleFor(x => x.PhoneNumber)
                .Must(p => string.IsNullOrWhiteSpace(p) || Regex.IsMatch(p, @"^\+?\d{9,15}$"))
                .WithMessage("Phone number must be in E.164 format (e.g., +84901234567).");
        }

        private bool BeValidDateFormat(string? date)
        {
            if (string.IsNullOrEmpty(date))
                return true;

            return DateTime.TryParseExact(
                date,
                "yyyy-MM-dd",
                CultureInfo.InvariantCulture,
                DateTimeStyles.None,
                out _
            );
        }
    }
}
