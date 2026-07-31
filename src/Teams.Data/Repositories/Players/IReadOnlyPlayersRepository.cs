using Teams.Data.Models;
using Teams.Domain.Entities;
using Teams.Domain.Enums;

namespace Teams.Data.Repositories.Players;

/// <summary>Describes a read-only repository containing instances of <see cref="Player"/>.</summary>
public interface IReadOnlyPlayersRepository : IReadOnlyRepository<Player>
{
    /// <summary>Get a collection of players matching the given filters.</summary>
    /// <param name="gameId">Limit results to those associated with the game represented by this identifier.</param>
    /// <param name="displayName">Limit results to those with a display name containing this value.</param>
    /// <param name="userId">Limit results to those associated with the user represented by this identifier.</param>
    /// <param name="rating">Limit results to those matching this range filter.</param>
    /// <param name="team">Limit result to those matching this team value.</param>
    /// <param name="type">Limit results to those matching this type value.</param>
    /// <param name="dateFilter">Limit results to those matching this date filter.</param>
    /// <param name="pagination">Limit results to a page matching this pagination filter.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns></returns>
    Task<IEnumerable<Player>> GetAsync(
        string? gameId = null,
        string? displayName = null,
        string? userId = null,
        RangeFilter<int>? rating = null,
        GameTeamEnum? team = null,
        PlayerTypeEnum? type = null,
        DateFilter? dateFilter = null,
        PaginationFilter? pagination = null,
        CancellationToken cancellationToken = default);
}