using FluentValidation;
using Teams.Domain;

namespace Teams.Core.UseCases.Games.GenerateTeams;

public class GenerateTeamsCommandValidator : AbstractValidator<GenerateTeamsCommand>
{
    public GenerateTeamsCommandValidator()
    {
        RuleFor(r => r.Count)
            .InclusiveBetween(1, Constants.MaximumGeneratedTeamSuggestionCount);

        RuleFor(r => r.Differential)
            .GreaterThanOrEqualTo(100);
    }
}