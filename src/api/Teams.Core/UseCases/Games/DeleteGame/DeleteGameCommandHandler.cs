using Microsoft.Extensions.Logging;
using Teams.Core.CQRS;
using Teams.Core.Exceptions;
using Teams.Core.Services;
using Teams.Data.Services;
using Teams.Domain.Entities;

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

        game.Delete();

        await uow.Games.UpdateAsync(game, cancellationToken);
        await uow.SaveChangesAsync(cancellationToken);

        logger.LogInformation("Game deleted: {game}", game);
        return game;
    }
}