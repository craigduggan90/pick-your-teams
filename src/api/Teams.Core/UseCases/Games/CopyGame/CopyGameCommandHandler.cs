using FluentValidation.Results;
using Microsoft.Extensions.Logging;
using Teams.Core.CQRS;
using Teams.Core.Exceptions;
using Teams.Core.Services;
using Teams.Core.Services.Events;
using Teams.Core.UseCases.Invitations.CreateInvitations;
using Teams.Data.Services;
using Teams.Domain.Entities;
using Teams.Domain.Enums;

namespace Teams.Core.UseCases.Games.CopyGame;

public class CopyGameCommandHandler(
    IUnitOfWork uow,
    IActorAccessor actor,
    IEventPublisher publisher,
    ILogger<CopyGameCommandHandler> logger) : IRequestHandler<CopyGameCommand, Game>
{
    public async Task<Game> HandleAsync(CopyGameCommand request, CancellationToken cancellationToken)
    {
        var source = await uow.Games.GetByIdAsync(request.Id, cancellationToken)
            ?? throw new NotFoundException(typeof(Game), request.Id);

        actor.Current.ThrowIfNotOrganiser(source.OrganiserId);

        if (source.Status != GameStatusEnum.Finished)
            throw new CommandValidationException([new ValidationFailure(nameof(CopyGameCommand.Id), "Only a finished game can be copied.")]);

        var copy = await uow.Games.CreateAsync(
            new Game(actor.Current.Id, source.Location, request.StartTime, source.Duration, source.TeamSize),
            cancellationToken);

        var invitationEvents = new List<InvitationCreatedEvent>();
        foreach (var player in source.Players)
            await CopyPlayerAsync(copy, player, invitationEvents, cancellationToken);

        await uow.SaveChangesAsync(cancellationToken);
        await publisher.PublishEventsAsync(invitationEvents, cancellationToken);

        logger.LogInformation("Game copied: {source} -> {copy}", source.Id, copy.Id);
        return copy;
    }

    // Dummy players carry straight over - the organiser is already vouching for them, same as
    // Add Non-User Player. Real users are re-invited rather than added directly, matching the
    // only path by which a user ever joins a game elsewhere in the app (Invite Players -> accept)
    // - a copy shouldn't silently re-enrol someone without their consent just because they were on
    // the original roster.
    private async Task CopyPlayerAsync(
        Game copy, Player player, List<InvitationCreatedEvent> invitationEvents, CancellationToken cancellationToken)
    {
        if (player.Type == PlayerTypeEnum.Dummy)
        {
            var rating = player.Rating + (player.RatingChange ?? 0);
            await uow.Players.CreateAsync(new Player(copy, player.DisplayName!, rating), cancellationToken);
            return;
        }

        if (player.User is null)
        {
            logger.LogWarning("Skipped inviting player {player}: associated user has been deleted.", player.Id);
            return;
        }

        var invitation = await uow.Invitations.CreateAsync(
            new Invitation(copy.Id, player.User.Id, player.User.EmailAddress), cancellationToken);
        invitationEvents.Add(new InvitationCreatedEvent(invitation.Id, copy.Id, player.User.Id));
    }
}