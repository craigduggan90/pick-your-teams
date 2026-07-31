using Microsoft.Extensions.Logging;
using Teams.Core.CQRS;
using Teams.Core.Exceptions;
using Teams.Data.Services;
using Teams.Domain.Entities;

namespace Teams.Core.UseCases.Players.DeletePlayer;

public class DeletePlayerCommandHandler(IUnitOfWork uow, ILogger<DeletePlayerCommandHandler> logger)
    : IRequestHandler<DeletePlayerCommand, Player>
{
    public async Task<Player> HandleAsync(DeletePlayerCommand request, CancellationToken cancellationToken)
    {
        var player = await uow.Players.GetByIdAsync(request.Id, cancellationToken)
               ?? throw new NotFoundException(typeof(Player), request.Id);

        player.Delete();

        await uow.Players.UpdateAsync(player, cancellationToken);
        await uow.SaveChangesAsync(cancellationToken);
        logger.LogInformation("Player deleted: {id}", request.Id);

        return player;
    }
}