using FluentValidation;
using Teams.Domain.Enums;
using Teams.Core.Services.Jobs.Requests;

namespace Teams.Core.Services.Jobs.Validators;

public class UpdateJobRequestValidator : AbstractValidator<UpdateJobRequest>
{
    public UpdateJobRequestValidator()
    {
        RuleFor(request => request.Id)
            .NotEmpty();

        RuleFor(request => request.Status)
            .Must(value => Enum.TryParse<JobStatusEnum>(value, true, out _))
            .WithMessage("Must contain a valid Status.");

        RuleFor(request => request.ErrorCode)
            .MaximumLength(100);

        RuleFor(request => request.ErrorCode)
            .NotEmpty()
            .When(request => string.Equals(request.Status, nameof(JobStatusEnum.Failed), StringComparison.OrdinalIgnoreCase))
            .WithMessage("ErrorCode is required when Status is Failed.");

        RuleFor(request => request.ErrorCode)
            .Empty()
            .When(request => !string.Equals(request.Status, nameof(JobStatusEnum.Failed), StringComparison.OrdinalIgnoreCase))
            .WithMessage("ErrorCode may only be applied when Status is Failed.");

        RuleFor(request => request.ErrorMessage)
            .MaximumLength(255);

        RuleFor(request => request.ErrorMessage)
            .NotEmpty()
            .When(request => string.Equals(request.Status, nameof(JobStatusEnum.Failed), StringComparison.OrdinalIgnoreCase))
            .WithMessage("ErrorMessage is required when Status is Failed.");

        RuleFor(request => request.ErrorMessage)
            .Empty()
            .When(request => !string.Equals(request.Status, nameof(JobStatusEnum.Failed), StringComparison.OrdinalIgnoreCase))
            .WithMessage("ErrorMessage may only be applied when Status is Failed.");
    }
}