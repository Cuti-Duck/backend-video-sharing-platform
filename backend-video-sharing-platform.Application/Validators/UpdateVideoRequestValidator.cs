using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using backend_video_sharing_platform.Application.DTOs.Video;
using FluentValidation;

namespace backend_video_sharing_platform.Application.Validators
{
    public class UpdateVideoRequestValidator : AbstractValidator<UpdateVideoRequest>
    {
        public UpdateVideoRequestValidator()
        {
            When(x => x.Title != null, () =>
            {
                RuleFor(x => x.Title)
                    .NotEmpty().WithMessage("Title must not be empty.")
                    .MaximumLength(200).WithMessage("Title must not exceed 200 characters.");
            });

            When(x => x.Description != null, () =>
            {
                RuleFor(x => x.Description)
                    .MaximumLength(2000).WithMessage("Description must not exceed 2000 characters.");
            });
        }
    }
}
