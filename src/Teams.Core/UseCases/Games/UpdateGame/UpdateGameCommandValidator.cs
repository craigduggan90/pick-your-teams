using FluentValidation;

namespace Teams.Core.UseCases.Games.UpdateGame;

public class UpdateGameCommandValidator : AbstractValidator<UpdateGameCommand>
{
    public UpdateGameCommandValidator()
    {
        RuleFor(game => game.Location)
            .MaximumLength(100);

        RuleFor(game => game.Duration)
            .InclusiveBetween(15, 120);
    }
}