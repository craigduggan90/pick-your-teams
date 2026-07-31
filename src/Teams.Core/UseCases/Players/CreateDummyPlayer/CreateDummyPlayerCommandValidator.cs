using FluentValidation;

namespace Teams.Core.UseCases.Players.CreateDummyPlayer;

public class CreateDummyPlayerCommandValidator : AbstractValidator<CreateDummyPlayerCommand>
{
    public CreateDummyPlayerCommandValidator()
    {
        RuleFor(command => command.DisplayName)
            .NotEmpty()
            .MaximumLength(100);

        RuleFor(command => command.EstimatedRating)
            .InclusiveBetween(1, 2000);
    }
}