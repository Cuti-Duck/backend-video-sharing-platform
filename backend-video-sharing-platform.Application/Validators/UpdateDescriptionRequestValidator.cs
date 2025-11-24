using backend_video_sharing_platform.Application.DTOs.Channel;
using FluentValidation;

namespace backend_video_sharing_platform.Application.Validators
{
    public class UpdateDescriptionRequestValidator : AbstractValidator<UpdateDescriptionRequest>
    {
        public UpdateDescriptionRequestValidator()
        {
            RuleFor(x => x.Description)
                .NotEmpty().WithMessage("Channel description is required.")
                .MaximumLength(500).WithMessage("Channel description must not exceed 500 characters.");
        }
    }
}
