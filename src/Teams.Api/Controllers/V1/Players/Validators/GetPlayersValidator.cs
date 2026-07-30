using FluentValidation;
using Teams.Api.Controllers.V1.Players.RequestModels;
using Teams.Common.Pagination;

namespace Teams.Api.Controllers.V1.Players.Validators;

public class GetPlayersValidator : AbstractValidator<GetPlayersRequestModel>
{
    public GetPlayersValidator()
    {
        RuleFor(request => request.Cursor)
            .Must(value => value.TryDecodeCursor(out _))
            .When(request => request.Cursor is not null)
            .WithMessage("Invalid cursor value.");

        RuleFor(request => request.PageSize)
            .GreaterThan(0)
            .LessThanOrEqualTo(100)
            .When(request => request.PageSize is not null);

        RuleFor(request => request.RatingFrom)
            .GreaterThan(0)
            .When(request => request.RatingFrom is not null);

        RuleFor(request => request.RatingTo)
            .GreaterThan(0)
            .When(request => request.RatingTo is not null);

        RuleFor(request => request.RatingFrom)
            .GreaterThan(request => request.RatingTo)
            .When(request => request.RatingFrom is not null && request.RatingTo is not null);
    }
}