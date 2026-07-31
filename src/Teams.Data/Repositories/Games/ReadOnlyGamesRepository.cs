using Microsoft.EntityFrameworkCore;
using Teams.Data.Context;
using Teams.Data.Filters;
using Teams.Data.Models;
using Teams.Domain.Entities;

namespace Teams.Data.Repositories.Games;

/// <summary>A read-only repository containing instances of <see cref="Game"/>.</summary>
public class ReadOnlyGamesRepository(ApiDbContext context) : RepositoryBase(context), IReadOnlyGamesRepository
{
    /// <inheritdoc />
    public async Task<Game?> GetByIdAsync(string id, CancellationToken cancellationToken)
        => await Context.Games
            .Include(g => g.Players)
                .ThenInclude(gp => gp.User)
            .SingleOrDefaultAsync(entity => entity.Id == id, cancellationToken);

    /// <inheritdoc />
    public async Task<IEnumerable<Game>> GetAsync(
        string? location = null,
        RangeFilter<DateTime>? startTime = null,
        RangeFilter<int>? duration = null,
        int? teamSize = null,
        DateFilter? dateFilter = null,
        PaginationFilter? pagination = null,
        CancellationToken cancellationToken = default)
        => await Context.Games
            .ApplyLocationFilter(location)
            .ApplyStartTimeFromFilter(startTime?.From)
            .ApplyStartTimeToFilter(startTime?.To)
            .ApplyDurationFromFilter(duration?.From)
            .ApplyDurationToFilter(duration?.To)
            .ApplyTeamSizeFilter(teamSize)
            .ApplyCreatedFromFilter(dateFilter?.Created?.From)
            .ApplyCreatedToFilter(dateFilter?.Created?.To)
            .ApplyModifiedFromFilter(dateFilter?.Modified?.From)
            .ApplyModifiedToFilter(dateFilter?.Modified?.To)
            .ApplyCursor(pagination?.Cursor)
            .ApplyPagination(pagination?.PageSize ?? Constants.DefaultPageSize)
            .ToListAsync(cancellationToken);
}