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
            // Name — optional nhưng không được quá dài
            RuleFor(x => x.Name)
                .MaximumLength(50).WithMessage("Tên không được vượt quá 50 ký tự.");

            // Gender — chỉ male hoặc female (case-insensitive)
            RuleFor(x => x.Gender)
                .Must(g => string.IsNullOrEmpty(g) || g.Equals("male", StringComparison.OrdinalIgnoreCase) || g.Equals("female", StringComparison.OrdinalIgnoreCase))
                .WithMessage("Giới tính chỉ được là 'male' hoặc 'female'.");

            // BirthDate — đúng format yyyy-MM-dd
            RuleFor(x => x.BirthDate)
                .Must(BeValidDateFormat)
                .WithMessage("Ngày sinh phải có định dạng yyyy-MM-dd.");

            // PhoneNumber — theo định dạng +849xxxxxxxx (VN)
            RuleFor(x => x.PhoneNumber)
                .Must(p => string.IsNullOrWhiteSpace(p) || Regex.IsMatch(p, @"^\+?\d{9,15}$"))
                .WithMessage("PhoneNumber must be E.164 format (e.g. +84901234567).");
        }

        private bool BeValidDateFormat(string? date)
        {
            if (string.IsNullOrEmpty(date))
                return true; // Optional field

            return DateTime.TryParseExact(date, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out _);
        }
    }
}
