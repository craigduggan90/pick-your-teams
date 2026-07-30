using Teams.Common.Pagination;
using Teams.Core.Services.Players.Commands;
using Teams.Core.Services.Players.Queries;
using Teams.Domain.Entities;

namespace Teams.Core.Services.Players;

public interface IPlayersService
{
    Task<PagedList<Player>> GetPlayersAsync(GetPlayersQuery query, CancellationToken cancellationToken);

    Task<Player> GetPlayerByIdAsync(GetPlayerByIdQuery query, CancellationToken cancellationToken);

    Task<Player> CreatePlayerAsync(CreatePlayerCommand command, CancellationToken cancellationToken);

    Task UpdatePlayerAsync(UpdatePlayerCommand command, CancellationToken cancellationToken);

    Task DeletePlayerAsync(DeletePlayerCommand command, CancellationToken cancellationToken);
}