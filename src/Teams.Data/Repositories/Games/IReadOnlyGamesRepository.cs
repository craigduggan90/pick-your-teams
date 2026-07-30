using Teams.Data.Models;
using Teams.Domain.Entities;

namespace Teams.Data.Repositories.Games;

/// <summary>Describes a read-only repository containing instances of <see cref="Game"/>.</summary>
public interface IReadOnlyGamesRepository : IReadOnlyRepository<Game>
{
    /// <summary>Get a collection of players with optional filters applied.</summary>
    /// <param name="location">Limit results to records matching this location filter.</param>
    /// <param name="startTime">Limit results to records matching this start time filter.</param>
    /// <param name="endTime">Limit results to records matching this end time filter.</param>
    /// <param name="teamSize">Limit results to records matching this team size filter.</param>
    /// <param name="dateFilter">Limit results to records matching this date filter.</param>
    /// <param name="pagination">Limit results to a page matching this pagination filter.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    Task<IEnumerable<Game>> GetAsync(
        string? location = null,
        RangeFilter<DateTime>? startTime = null,
        RangeFilter<DateTime>? endTime = null,
        int? teamSize = null,
        DateFilter? dateFilter = null,
        PaginationFilter? pagination = null,
        CancellationToken cancellationToken = default);
}