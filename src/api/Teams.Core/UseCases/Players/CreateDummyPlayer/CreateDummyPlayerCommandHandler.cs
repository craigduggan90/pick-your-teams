using FluentValidation;
using FluentValidation.Results;
using Microsoft.Extensions.Logging;
using Teams.Core.CQRS;
using Teams.Core.Exceptions;
using Teams.Core.Services;
using Teams.Data.Services;
using Teams.Domain.Entities;

namespace Teams.Core.UseCases.Players.CreateDummyPlayer;

public class CreateDummyPlayerCommandHandler(
    IUnitOfWork uow,
    IActorAccessor actor,
    IValidator<CreateDummyPlayerCommand> validator,
    ILogger<CreateDummyPlayerCommandHandler> logger) : IRequestHandler<CreateDummyPlayerCommand, Player>
{
    public async Task<Player> HandleAsync(CreateDummyPlayerCommand request, CancellationToken cancellationToken)
    {
        // Validate the request
        CommandValidationException.ThrowIfValidationFailed(await validator.ValidateAsync(request, cancellationToken));

        // Check that the game exists
        var game = await uow.Games.GetByIdAsync(request.GameId, cancellationToken)
            ?? throw new NotFoundException(typeof(Game), request.GameId);

        actor.Current.ThrowIfNotOrganiser(game.OrganiserId);

        if (game.Players.Count >= game.MaxPlayers)
            throw new CommandValidationException([new ValidationFailure(nameof(CreateDummyPlayerCommand.GameId), "Game has reached its maximum number of players.")]);

        // Create the player
        var player = await uow.Players.CreateAsync(
            new Player(game, request.DisplayName, request.EstimatedRating),
            cancellationToken);

        // Commit the changes
        await uow.SaveChangesAsync(cancellationToken);

        logger.LogInformation("Player created (dummy): {id}", player.Id);
        return player;
    }
}