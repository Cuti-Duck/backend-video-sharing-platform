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
            // Email — bắt buộc, đúng format
            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("Email không được để trống.")
                .EmailAddress().WithMessage("Email không hợp lệ.");

            // Password — bắt buộc, đủ mạnh
            RuleFor(x => x.Password)
                .NotEmpty().WithMessage("Mật khẩu không được để trống.")
                .MinimumLength(8).WithMessage("Mật khẩu phải có ít nhất 8 ký tự.")
                .Matches("[A-Z]").WithMessage("Mật khẩu phải chứa ít nhất 1 chữ in hoa.")
                .Matches("[a-z]").WithMessage("Mật khẩu phải chứa ít nhất 1 chữ thường.")
                .Matches("[0-9]").WithMessage("Mật khẩu phải chứa ít nhất 1 số.")
                .Matches("[^a-zA-Z0-9]").WithMessage("Mật khẩu phải chứa ít nhất 1 ký tự đặc biệt.");

            // Name — bắt buộc, không quá dài
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Tên không được để trống.")
                .MaximumLength(50).WithMessage("Tên không được vượt quá 50 ký tự.");

            // Gender — chỉ male hoặc female
            RuleFor(x => x.Gender)
                .Must(g => string.IsNullOrEmpty(g)
                    || g.Equals("male", StringComparison.OrdinalIgnoreCase)
                    || g.Equals("female", StringComparison.OrdinalIgnoreCase))
                .WithMessage("Giới tính chỉ được là 'male' hoặc 'female'.");

            // BirthDate — đúng định dạng yyyy-MM-dd
            RuleFor(x => x.BirthDate)
                .NotEmpty().WithMessage("Ngày sinh không được để trống.")
                .Must(BeValidDateFormat)
                .WithMessage("Ngày sinh phải có định dạng yyyy-MM-dd.");

            // PhoneNumber — đúng format quốc tế (E.164)
            RuleFor(x => x.PhoneNumber)
                .Must(p => string.IsNullOrWhiteSpace(p) || Regex.IsMatch(p, @"^\+?\d{9,15}$"))
                .WithMessage("Số điện thoại phải đúng định dạng E.164 (vd: +84901234567).");
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
