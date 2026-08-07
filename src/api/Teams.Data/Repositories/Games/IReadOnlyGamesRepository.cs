using Teams.Data.Models;
using Teams.Domain.Entities;
using Teams.Domain.Enums;

namespace Teams.Data.Repositories.Games;

/// <summary>Describes a read-only repository containing instances of <see cref="Game"/>.</summary>
public interface IReadOnlyGamesRepository : IReadOnlyRepository<Game>
{
    /// <summary>Get a collection of players with optional filters applied.</summary>
    /// <param name="location">Limit results to records matching this location filter.</param>
    /// <param name="startTime">Limit results to records matching this start time filter.</param>
    /// <param name="duration">Limit results to records matching this duration filter.</param>
    /// <param name="teamSize">Limit results to records matching this team size filter.</param>
    /// <param name="status">Limit results to records matching this status.</param>
    /// <param name="dateFilter">Limit results to records matching this date filter.</param>
    /// <param name="pagination">Limit results to a page matching this pagination filter.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <param name="organiserId">Limit results to records organised by the user with this identifier.</param>
    /// <param name="userId">Limit results to records in which this user is a player.</param>
    Task<IEnumerable<Game>> GetAsync(
        string? location = null,
        RangeFilter<DateTime>? startTime = null,
        RangeFilter<int>? duration = null,
        int? teamSize = null,
        GameStatusEnum? status = null,
        string? organiserId = null,
        string? userId = null,
        DateFilter? dateFilter = null,
        PaginationFilter? pagination = null,
        CancellationToken cancellationToken = default);
}