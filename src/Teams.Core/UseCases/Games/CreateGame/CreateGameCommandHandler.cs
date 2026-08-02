using FluentValidation;
using Microsoft.Extensions.Logging;
using Teams.Core.CQRS;
using Teams.Core.Exceptions;
using Teams.Data.Services;
using Teams.Domain.Entities;

namespace Teams.Core.UseCases.Games.CreateGame;

public class CreateGameCommandHandler(
    IUnitOfWork uow,
    IValidator<CreateGameCommand> validator,
    ILogger<CreateGameCommandHandler> logger) : IRequestHandler<CreateGameCommand, Game>
{
    public async Task<Game> HandleAsync(CreateGameCommand request, CancellationToken cancellationToken)
    {
        var validation = await validator.ValidateAsync(request, cancellationToken);
        CommandValidationException.ThrowIfValidationFailed(validation);

        // TODO: use context user, or should that come from the presentation layer once it's built in (yes)?
        var game = new Game(request.Location, request.StartTime, request.Duration, request.TeamSize, "");
        await uow.Games.CreateAsync(game, cancellationToken);
        await uow.SaveChangesAsync(cancellationToken);

        logger.LogInformation("Game created: {game}", game);
        return game;
    }
}