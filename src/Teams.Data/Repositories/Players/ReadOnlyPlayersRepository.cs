using Microsoft.EntityFrameworkCore;
using Teams.Data.Context;
using Teams.Data.Filters;
using Teams.Data.Models;
using Teams.Domain.Entities;

namespace Teams.Data.Repositories.Players;

/// <summary>A read-only repository containing instances of <see cref="Player"/>.</summary>
public class ReadOnlyPlayersRepository(ApiDbContext context) : RepositoryBase(context), IReadOnlyPlayersRepository
{
    /// <inheritdoc />
    public async Task<Player?> GetByIdAsync(string id, CancellationToken cancellationToken)
        => await Context.Players
            .SingleOrDefaultAsync(entity => entity.Id == id, cancellationToken);

    /// <inheritdoc />
    public async Task<IEnumerable<Player>> GetAsync(
        string? name = null,
        RangeFilter<int>? rating = null,
        DateFilter? dateFilter = null,
        PaginationFilter? pagination = null,
        CancellationToken cancellationToken = default)
        => await Context.Players
            .ApplyNameFilter(name)
            .ApplyRatingFromFilter(rating?.From)
            .ApplyRatingToFilter(rating?.To)
            .ApplyCreatedFromFilter(dateFilter?.Created?.From)
            .ApplyCreatedToFilter(dateFilter?.Created?.To)
            .ApplyModifiedFromFilter(dateFilter?.Modified?.From)
            .ApplyModifiedToFilter(dateFilter?.Modified?.To)
            .ApplyCursor(pagination?.Cursor)
            .ApplyPagination(pagination?.PageSize ?? Constants.DefaultPageSize)
            .ToListAsync(cancellationToken);
}