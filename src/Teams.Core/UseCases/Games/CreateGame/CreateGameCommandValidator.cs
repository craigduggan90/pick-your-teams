using FluentValidation;

namespace Teams.Core.UseCases.Games.CreateGame;

public class CreateGameCommandValidator : AbstractValidator<CreateGameCommand>
{
    public CreateGameCommandValidator()
    {
        RuleFor(game => game.TeamSize)
            .InclusiveBetween(3, 11);

        RuleFor(game => game.Location)
            .MaximumLength(100);

        RuleFor(game => game.Duration)
            .InclusiveBetween(15, 120);
    }
}