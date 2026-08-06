using FluentValidation;

namespace Teams.Core.UseCases.Games.SetTeams;

public class SetTeamsCommandValidator : AbstractValidator<SetTeamsCommand>
{
    internal const string DuplicatePlayer = "Duplicate player identifier in team.";
    internal const string PlayerOnBothTeams = "Player assigned to both teams.";

    public SetTeamsCommandValidator()
    {
        RuleFor(command => command.HomeTeamIds)
            .Must(list => list.Count == list.Distinct(StringComparer.OrdinalIgnoreCase).Count())
            .WithMessage(DuplicatePlayer);

        RuleFor(command => command.AwayTeamIds)
            .Must(list => list.Count == list.Distinct(StringComparer.OrdinalIgnoreCase).Count())
            .WithMessage(DuplicatePlayer);

        RuleForEach(command => command.AwayTeamIds)
            .Must((command, value, _) => !command.HomeTeamIds.Contains(value, StringComparer.OrdinalIgnoreCase))
            .WithMessage(PlayerOnBothTeams);
    }
}