using FluentValidation.Results;
using Microsoft.Extensions.Logging;
using Teams.Core.CQRS;
using Teams.Core.Exceptions;
using Teams.Core.Services;
using Teams.Data.Services;
using Teams.Domain.Entities;

namespace Teams.Core.UseCases.Players.CreatePlayer;

public class CreatePlayerCommandHandler(
    IUnitOfWork uow,
    IActorAccessor actor,
    ILogger<CreatePlayerCommandHandler> logger) : IRequestHandler<CreatePlayerCommand, Player>
{
    public async Task<Player> HandleAsync(CreatePlayerCommand request, CancellationToken cancellationToken)
    {
        // Check that the game exists
        var game = await uow.Games.GetByIdAsync(request.GameId, cancellationToken)
            ?? throw new NotFoundException(typeof(Game), request.GameId);

        actor.Current.ThrowIfNotOrganiserOrUser(request.UserId, game.OrganiserId);

        if (game.Players.Any(player => player.UserId == request.UserId))
            throw new CommandValidationException([new ValidationFailure(nameof(CreatePlayerCommand.UserId), "User is already associated with game.")]);

        var user = await uow.Users.GetByIdAsync(request.UserId, cancellationToken)
                   ?? throw new NotFoundException(typeof(User), request.UserId);

        var player = await uow.Players.CreateAsync(
            new Player(game, user),
            cancellationToken);

        // Commit the changes
        await uow.SaveChangesAsync(cancellationToken);

        logger.LogInformation("Player created: {id} (User: {userId})", player.Id, player.UserId);
        return player;
    }
}