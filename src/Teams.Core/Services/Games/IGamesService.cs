using Teams.Common.Pagination;
using Teams.Core.Services.Games.Commands;
using Teams.Core.Services.Games.Queries;
using Teams.Domain.Entities;

namespace Teams.Core.Services.Games;

public interface IGamesService
{
    Task<PagedList<Game>> GetGamesAsync(GetGamesQuery query, CancellationToken cancellationToken);

    Task<Game> GetGameByIdAsync(GetGameByIdQuery query, CancellationToken cancellationToken);

    Task<Game> CreateGameAsync(CreateGameCommand command, CancellationToken cancellationToken);

    Task UpdateGameAsync(UpdateGameCommand command, CancellationToken cancellationToken);

    Task SetGameResultAsync(RecordGameResultCommand command, CancellationToken cancellationToken);

    Task DeleteGameAsync(DeleteGameCommand command, CancellationToken cancellationToken);
}