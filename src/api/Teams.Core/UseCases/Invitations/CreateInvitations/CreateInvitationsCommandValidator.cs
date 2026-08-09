using FluentValidation;
using Teams.Common.Extensions;

namespace Teams.Core.UseCases.Invitations.CreateInvitations;

public class CreateInvitationsCommandValidator : AbstractValidator<CreateInvitationsCommand>
{
    public CreateInvitationsCommandValidator()
    {
        RuleFor(command => command.UserTags)
            .NotEmpty()
            .WithMessage("At least one tag must be provided.");

        RuleFor(command => command.UserTags)
            .Must(tags => tags.Distinct(StringComparer.OrdinalIgnoreCase).Count() == tags.Count)
            .WithMessage("Duplicate tags provided.");

        // A single Must check (rather than NotEmpty + MinimumLength + Matches as separate rules) avoids stacking
        // three messages for the same bad value - matches the resolution used for InvitePlayers.
        RuleForEach(command => command.UserTags)
            .Must(tag => tag?.Length is >= 3 and <= 36 && tag.IsValidTag())
            .WithMessage("'{PropertyValue}' is not a valid tag.");
    }
}