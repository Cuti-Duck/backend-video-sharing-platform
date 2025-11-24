using System.Globalization;
using System.Text.RegularExpressions;
using backend_video_sharing_platform.Application.DTOs.User;
using FluentValidation;

namespace backend_video_sharing_platform.Application.Validators
{
    public class UpdateUserRequestValidator : AbstractValidator<UpdateUserRequest>
    {
        public UpdateUserRequestValidator()
        {
            // Name — optional but must not be too long
            RuleFor(x => x.Name)
                .MaximumLength(50).WithMessage("Name must not exceed 50 characters.");

            // Gender — only male or female (case-insensitive)
            RuleFor(x => x.Gender)
                .Must(g => string.IsNullOrEmpty(g)
                    || g.Equals("male", StringComparison.OrdinalIgnoreCase)
                    || g.Equals("female", StringComparison.OrdinalIgnoreCase))
                .WithMessage("Gender must be either 'male' or 'female'.");

            // BirthDate — correct format yyyy-MM-dd
            RuleFor(x => x.BirthDate)
                .Must(BeValidDateFormat)
                .WithMessage("Birth date must follow the yyyy-MM-dd format.");

            // PhoneNumber — E.164 format
            RuleFor(x => x.PhoneNumber)
                .Must(p => string.IsNullOrWhiteSpace(p) || Regex.IsMatch(p, @"^\+?\d{9,15}$"))
                .WithMessage("Phone number must be in E.164 format (e.g. +84901234567).");
        }

        private bool BeValidDateFormat(string? date)
        {
            if (string.IsNullOrEmpty(date))
                return true; // Optional field

            return DateTime.TryParseExact(
                date,
                "yyyy-MM-dd",
                CultureInfo.InvariantCulture,
                DateTimeStyles.None,
                out _);
        }
    }
}
