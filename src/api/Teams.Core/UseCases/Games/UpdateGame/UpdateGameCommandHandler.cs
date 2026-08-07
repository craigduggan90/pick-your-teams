using FluentValidation;
using Microsoft.Extensions.Logging;
using Teams.Core.CQRS;
using Teams.Core.Exceptions;
using Teams.Core.Services;
using Teams.Data.Services;
using Teams.Domain.Entities;

namespace Teams.Core.UseCases.Games.UpdateGame;

public class UpdateGameCommandHandler(
    IUnitOfWork uow,
    IActorAccessor actor,
    IValidator<UpdateGameCommand> validator,
    ILogger<UpdateGameCommandHandler> logger) : IRequestHandler<UpdateGameCommand, Game>
{
    public async Task<Game> HandleAsync(UpdateGameCommand request, CancellationToken cancellationToken)
    {
        var validation = await validator.ValidateAsync(request, cancellationToken);
        CommandValidationException.ThrowIfValidationFailed(validation);

        var game = await uow.Games.GetByIdAsync(request.Id, cancellationToken)
                   ?? throw new NotFoundException(typeof(Game), request.Id);

        actor.Current.ThrowIfNotOrganiser(game.OrganiserId);

        game.Update(request.Location, request.StartTime, request.Duration);
        if (!game.IsDirty)
            return game;

        await uow.Games.UpdateAsync(game, cancellationToken);
        await uow.SaveChangesAsync(cancellationToken);

        logger.LogInformation("Game updated: {game}", game);
        return game;
    }
}