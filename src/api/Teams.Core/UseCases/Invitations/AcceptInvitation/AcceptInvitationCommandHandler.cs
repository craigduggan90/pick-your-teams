using Microsoft.Extensions.Logging;
using Teams.Core.CQRS;
using Teams.Core.Exceptions;
using Teams.Core.Services;
using Teams.Core.Services.Events;
using Teams.Data.Services;
using Teams.Domain.Entities;
using Teams.Domain.Enums;

namespace Teams.Core.UseCases.Invitations.AcceptInvitation;

public class AcceptInvitationCommandHandler(
    IUnitOfWork uow,
    IActorAccessor actor,
    IEventPublisher publisher,
    ILogger<AcceptInvitationCommandHandler> logger) : IRequestHandler<AcceptInvitationCommand, Invitation>
{
    public async Task<Invitation> HandleAsync(AcceptInvitationCommand request, CancellationToken cancellationToken)
    {
        var invitation = await uow.Invitations.GetByIdAsync(request.Id, cancellationToken)
                         ?? throw new NotFoundException(typeof(Invitation), request.Id);

        // Check that the user is operating on their own invitation
        actor.Current.ThrowIfNotUser(invitation.User?.Id ?? string.Empty);

        // If the invitation's already been accepted, just return it - no change
        if (invitation.Status == InvitationStatusEnum.Accepted)
        {
            logger.LogInformation("Invitation already accepted: {id}", invitation.Id);
            return invitation;
        }

        if (invitation.Status != InvitationStatusEnum.Open)
            throw RequestHandlerException.ForCommandRequest($"Invitation already claimed (Status: {invitation.Status})");

        // If the player is already in the game, mark the invitation as errored
        if (invitation.Game.Players.Any(player => invitation.UserId!.Equals(player.UserId, StringComparison.OrdinalIgnoreCase)))
        {
            logger.LogInformation("Invitation for game in which player is already recorded: {id}", invitation.Id);
            invitation.DispatchError("Unable to accept: player already in game.");
            await uow.Invitations.UpdateAsync(invitation, cancellationToken);
            await uow.SaveChangesAsync(cancellationToken);
            return invitation;
        }

        if (invitation.Game.Players.Count >= invitation.Game.MaxPlayers)
            throw RequestHandlerException.ForCommandRequest("Unable to accept: game is already at capacity.");

        invitation.Accept();
        await uow.Invitations.UpdateAsync(invitation, cancellationToken);

        var player = new Player(invitation.Game, invitation.User!);
        await uow.Players.CreateAsync(player, cancellationToken);

        await uow.SaveChangesAsync(cancellationToken);
        await publisher.PublishEventAsync(new InvitationAcceptedEvent(invitation.Id), cancellationToken);

        logger.LogInformation("Invitation updated: {invitation}", invitation);
        logger.LogInformation("Player created: {player}", player);

        return invitation;
    }
}