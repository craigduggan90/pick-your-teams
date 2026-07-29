using FluentValidation;
using Teams.Common.Pagination;
using Teams.Domain.Enums;
using Teams.Core.Services.Jobs.Requests;

namespace Teams.Core.Services.Jobs.Validators;

public class GetJobsRequestValidator : AbstractValidator<GetJobsRequest>
{
    public GetJobsRequestValidator()
    {
        RuleFor(request => request.Cursor)
            .Must(value => value.TryDecodeCursor(out _))
            .When(request => request.Cursor is not null)
            .WithMessage("Invalid cursor value.");

        RuleFor(request => request.Type)
            .Must(value => Enum.TryParse<JobTypeEnum>(value, true, out _))
            .When(request => request.Type is not null)
            .WithMessage("Must contain a valid Type.");

        RuleFor(request => request.Status)
            .Must(value => Enum.TryParse<JobStatusEnum>(value, true, out _))
            .When(request => request.Status is not null)
            .WithMessage("Must contain a valid Status.");

        RuleFor(request => request.PageSize)
            .GreaterThan(0)
            .LessThanOrEqualTo(100)
            .When(request => request.PageSize is not null);
    }
}