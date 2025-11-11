using backend_video_sharing_platform.Application.DTOs.Channel;
using FluentValidation;

namespace backend_video_sharing_platform.Application.Validators
{
    public class UpdateDescriptionRequestValidator : AbstractValidator<UpdateDescriptionRequest>
    {
        public UpdateDescriptionRequestValidator()
        {
            RuleFor(x => x.Description)
                .NotEmpty().WithMessage("Mô tả kênh không được để trống.")
                .MaximumLength(500).WithMessage("Mô tả kênh không được vượt quá 500 ký tự.");
        }
    }
}
