using Beans.Requestable;
using Microsoft.Extensions.Logging;
using Teams.Core.Exceptions;
using Teams.Core.Services;
using Teams.Data.Models;
using Teams.Data.Services;
using Teams.Domain.Entities;
using Teams.Domain.Enums;

namespace Teams.Core.UseCases.Games.DeleteGame;

public class DeleteGameCommandHandler(
    IUnitOfWork uow,
    IActorAccessor actor,
    ILogger<DeleteGameCommandHandler> logger) : IRequestHandler<DeleteGameCommand, Game>
{
    public async Task<Game> HandleAsync(DeleteGameCommand request, CancellationToken cancellationToken)
    {
        var game = await uow.Games.GetByIdAsync(request.Id, cancellationToken)
                   ?? throw new NotFoundException(typeof(Game), request.Id);

        actor.Current.ThrowIfNotOrganiser(game.OrganiserId);

        // Open invitations don't cascade with the game - a soft-deleted game's Include gets
        // filtered out of GetInvitationsAsync (its own query filter), but CountInvitationsAsync
        // never joins Game, so an untouched Open invitation would keep counting toward the
        // invitee's pending-invitations badge forever with no way to reach or clear it.
        var openInvitations = await uow.Invitations.GetInvitationsAsync(
            gameId: game.Id,
            status: InvitationStatusEnum.Open,
            pagination: new PaginationFilter(PageSize: int.MaxValue),
            cancellationToken: cancellationToken);

        foreach (var invitation in openInvitations)
        {
            invitation.DispatchError("Game was deleted.");
            await uow.Invitations.UpdateAsync(invitation, cancellationToken);
        }

        game.Delete();

        await uow.Games.UpdateAsync(game, cancellationToken);
        await uow.SaveChangesAsync(cancellationToken);

        logger.LogInformation("Game deleted: {game}", game);
        return game;
    }
}