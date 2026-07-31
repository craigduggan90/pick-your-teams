using FluentValidation;
using Microsoft.Extensions.Logging;
using Teams.Core.CQRS;
using Teams.Core.Exceptions;
using Teams.Data.Services;
using Teams.Domain.Entities;

namespace Teams.Core.UseCases.Players.CreateDummyPlayer;

public class CreateDummyPlayerCommandHandler(
    IUnitOfWork uow,
    IValidator<CreateDummyPlayerCommand> validator,
    ILogger<CreateDummyPlayerCommandHandler> logger) : IRequestHandler<CreateDummyPlayerCommand, Player>
{
    public async Task<Player> HandleAsync(CreateDummyPlayerCommand request, CancellationToken cancellationToken)
    {
        // Validate the request
        CommandValidationException.ThrowIfValidationFailed(await validator.ValidateAsync(request, cancellationToken));

        // Check that the game exists
        _ = await uow.Games.GetByIdAsync(request.GameId, cancellationToken)
            ?? throw new NotFoundException(typeof(Game), request.GameId);

        // Create the player
        var player = await uow.Players.CreateAsync(
            new Player(request.GameId, request.DisplayName, request.EstimatedRating),
            cancellationToken);

        // Commit the changes
        await uow.SaveChangesAsync(cancellationToken);

        logger.LogInformation("Player created (dummy): {id}", player.Id);
        return player;
    }
}