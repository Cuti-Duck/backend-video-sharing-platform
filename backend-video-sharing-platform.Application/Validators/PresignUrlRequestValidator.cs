using backend_video_sharing_platform.Application.DTOs;
using FluentValidation;

namespace backend_video_sharing_platform.Application.Validators
{
    public class PresignUrlRequestValidator : AbstractValidator<PresignUrlRequest>
    {
        public PresignUrlRequestValidator()
        {
            RuleFor(x => x.ChannelId)
                .NotEmpty().WithMessage("ChannelId is required.")
                .Must(BeValidGuid).WithMessage("ChannelId must be a valid GUID.");

            RuleFor(x => x.Title)
                .NotEmpty().WithMessage("Title is required.")
                .MaximumLength(100).WithMessage("Title cannot exceed 100 characters.");

            RuleFor(x => x.Description)
                .MaximumLength(500).WithMessage("Description cannot exceed 500 characters.")
                .When(x => !string.IsNullOrWhiteSpace(x.Description));
        }

        private bool BeValidGuid(string guid)
        {
            return Guid.TryParse(guid, out _);
        }
    }
}
