using Microsoft.Extensions.Logging;
using Teams.Core.CQRS;
using Teams.Core.Exceptions;
using Teams.Core.Services;
using Teams.Core.Services.Events;
using Teams.Data.Services;
using Teams.Domain.Entities;
using Teams.Domain.Enums;

namespace Teams.Core.UseCases.Invitations.DeclineInvitation;

public class DeclineInvitationCommandHandler(
    IUnitOfWork uow,
    IActorAccessor actor,
    IEventPublisher publisher,
    ILogger<DeclineInvitationCommandHandler> logger) : IRequestHandler<DeclineInvitationCommand, Invitation>
{
    public async Task<Invitation> HandleAsync(DeclineInvitationCommand request, CancellationToken cancellationToken)
    {
        var invitation = await uow.Invitations.GetByIdAsync(request.Id, cancellationToken)
                         ?? throw new NotFoundException(typeof(Invitation), request.Id);

        // Check that the user is operating on their own invitation
        actor.Current.ThrowIfNotUser(invitation.User?.Id ?? string.Empty);

        // If the invitation's already been declined, just return it - no change
        if (invitation.Status == InvitationStatusEnum.Declined)
        {
            logger.LogInformation("Invitation already declined: {id}", invitation.Id);
            return invitation;
        }

        if (invitation.Status != InvitationStatusEnum.Open)
            throw RequestHandlerException.ForCommandRequest($"Invitation already claimed (Status: {invitation.Status})");

        // This is open to debate - but if the player is already in the game, then declining this invitation does nothing
        // We simply mark it as errored the same way we do on the accept side.
        if (invitation.Game.Players.Any(player => invitation.UserId!.Equals(player.UserId, StringComparison.OrdinalIgnoreCase)))
        {
            logger.LogInformation("Invitation for game in which player is already recorded: {id}", invitation.Id);
            invitation.DispatchError("Unable to decline: player already in game.");
            await uow.Invitations.UpdateAsync(invitation, cancellationToken);
            await uow.SaveChangesAsync(cancellationToken);
            return invitation;
        }

        invitation.Decline();
        await uow.Invitations.UpdateAsync(invitation, cancellationToken);

        await uow.SaveChangesAsync(cancellationToken);
        await publisher.PublishEventAsync(new InvitationDeclinedEvent(invitation.Id), cancellationToken);

        logger.LogInformation("Invitation updated: {invitation}", invitation);

        return invitation;
    }
}