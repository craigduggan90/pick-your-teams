using FluentValidation;
using Teams.Core.Services.Jobs.Requests;
using Teams.Domain.Enums;

namespace Teams.Core.Services.Jobs.Validators;

public class CreateJobRequestValidator : AbstractValidator<CreateJobRequest>
{
    public CreateJobRequestValidator()
    {
        RuleFor(request => request.IdempotencyKey)
            .NotEmpty()
            .MaximumLength(100);

        RuleFor(request => request.Type)
            .Must(value => Enum.TryParse<JobTypeEnum>(value, true, out _))
            .WithMessage("Must contain a valid job type.");

        RuleFor(request => request.Parameters)
            .MaximumLength(1000);
    }
}