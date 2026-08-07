using Microsoft.EntityFrameworkCore;
using Teams.Data.Context;
using Teams.Data.Filters;
using Teams.Data.Models;
using Teams.Domain.Entities;
using Teams.Domain.Enums;

namespace Teams.Data.Repositories.Players;

/// <inheritdoc />
public class ReadOnlyPlayersRepository(ApiDbContext context)
    : RepositoryBase(context), IReadOnlyPlayersRepository
{
    /// <inheritdoc />
    public async Task<Player?> GetByIdAsync(string id, CancellationToken cancellationToken) =>
        await Context.Players
            .Include(p => p.Game)
            .SingleOrDefaultAsync(entity => entity.Id == id, cancellationToken);

    /// <inheritdoc />
    public async Task<IEnumerable<Player>> GetAsync(
        string? gameId = null,
        string? displayName = null,
        string? userId = null,
        RangeFilter<int>? rating = null,
        GameTeamEnum? team = null,
        PlayerTypeEnum? type = null,
        DateFilter? dateFilter = null,
        PaginationFilter? pagination = null,
        CancellationToken cancellationToken = default) =>
        await Context.Players
            .ApplyGameIdFilter(gameId)
            .ApplyDisplayNameFilter(displayName)
            .ApplyUserIdFilter(userId)
            .ApplyRatingFilter(rating)
            .ApplyTeamFilter(team)
            .ApplyTypeFilter(type)
            .ApplyBaseEntityDateFilters(dateFilter)
            .ApplyCursor(pagination?.Cursor)
            .ApplyPagination(pagination?.PageSize ?? Constants.DefaultPageSize)
            .ToListAsync(cancellationToken);
}