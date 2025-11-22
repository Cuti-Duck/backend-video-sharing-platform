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
                    .NotEmpty().WithMessage("Title không được để trống")
                    .MaximumLength(200);
            });

            When(x => x.Description != null, () =>
            {
                RuleFor(x => x.Description)
                    .MaximumLength(2000);
            });
        }
    }
}
