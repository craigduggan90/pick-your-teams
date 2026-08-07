using FluentValidation;
using Teams.Common.Extensions;

namespace Teams.Core.UseCases.Games.InvitePlayers;

public class InvitePlayersCommandValidator : AbstractValidator<InvitePlayersCommand>
{
    public InvitePlayersCommandValidator()
    {
        RuleFor(x => x.UserIdentifiers)
            .Must(list => list.Count <= 20)
            .WithMessage("Invited player list exceeds per-request limit.");

        RuleForEach(x => x.UserIdentifiers)
            .NotEmpty();

        RuleForEach(x => x.UserIdentifiers)
            .Must(ContainValidTagOrEmail)
            .WithMessage("Value must represent either a valid tag or email address.");
    }

    private static bool ContainValidTagOrEmail(string value)
        => value.IsValidEmail() || value.IsValidTag();
}