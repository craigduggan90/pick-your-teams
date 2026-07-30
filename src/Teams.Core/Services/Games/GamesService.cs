using Microsoft.Extensions.Logging;
using Teams.Common.Pagination;
using Teams.Core.Exceptions;
using Teams.Core.Services.Games.Commands;
using Teams.Core.Services.Games.Queries;
using Teams.Data.Repositories.Games;
using Teams.Data.Services;
using Teams.Domain.Entities;

namespace Teams.Core.Services.Games;

public class GamesService(
    IReadOnlyGamesRepository repository,
    IUnitOfWork unitOfWork,
    ILogger<GamesService> logger)
    : IGamesService
{
    public Task<PagedList<Game>> GetGamesAsync(GetGamesQuery query, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    public async Task<Game> GetGameByIdAsync(GetGameByIdQuery query, CancellationToken cancellationToken) =>
        await repository.GetByIdAsync(query.Id, cancellationToken) ??
        throw new NotFoundException(typeof(Game), query.Id);

    public async Task<Game> CreateGameAsync(CreateGameCommand command, CancellationToken cancellationToken)
    {
        var game = new Game(command.Location, command.StartTime, command.EndTime, command.TeamSize);
        await unitOfWork.Games.CreateAsync(game, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return game;
    }

    public async Task UpdateGameAsync(UpdateGameCommand command, CancellationToken cancellationToken)
    {
        var game = await unitOfWork.Games.GetByIdAsync(command.Id, cancellationToken)
                   ?? throw new NotFoundException(typeof(Game), command.Id);

        game.Update(command.Location, command.StartTime, command.EndTime);

        await unitOfWork.Games.UpdateAsync(game, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }

    public async Task SetGameResultAsync(RecordGameResultCommand command, CancellationToken cancellationToken)
    {
        var game = await unitOfWork.Games.GetByIdAsync(command.Id, cancellationToken)
                   ?? throw new NotFoundException(typeof(Game), command.Id);

        game.SetResult(command.Winner);

        // IRL: queue job to update player ratings
        // TODO: for now, we'll do that here - but call out to another service to do it.

        await unitOfWork.Games.UpdateAsync(game, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteGameAsync(DeleteGameCommand command, CancellationToken cancellationToken)
    {
        var game = await unitOfWork.Games.GetByIdAsync(command.Id, cancellationToken)
                   ?? throw new NotFoundException(typeof(Game), command.Id);

        game.Delete();

        await unitOfWork.Games.UpdateAsync(game, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}