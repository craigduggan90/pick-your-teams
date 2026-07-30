using Teams.Data.Models;
using Teams.Domain.Entities;

namespace Teams.Data.Repositories.Players;

/// <summary>Describes a read-only repository containing instances of <see cref="Player"/>.</summary>
public interface IReadOnlyPlayersRepository : IReadOnlyRepository<Player>
{
    /// <summary>Get a collection of players with optional filters applied.</summary>
    /// <param name="name">Limit results to records matching this name filter.</param>
    /// <param name="rating">Limit results to records matching this rating filter.</param>
    /// <param name="dateFilter">Limit results to records matching this date filter.</param>
    /// <param name="pagination">Limit results to a page matching this pagination filter.</param>
    /// <param name="cancellationToken">The cancellation token.</param>

    Task<IEnumerable<Player>> GetAsync(
        string? name = null,
        RangeFilter<int>? rating = null,
        DateFilter? dateFilter = null,
        PaginationFilter? pagination = null,
        CancellationToken cancellationToken = default);
}