using FluentValidation;
using Teams.Domain.Enums;

namespace Teams.Core.UseCases.Games.RecordResult;

public class RecordGameResultCommandValidator : AbstractValidator<RecordGameResultCommand>
{
    public RecordGameResultCommandValidator()
    {
        RuleFor(request => request.Winner)
            .Must(value => Enum.TryParse<GameTeamEnum>(value, true, out _))
            .WithMessage("Winner must contain a valid value [Home/Away/None].");
    }
}