using Microsoft.EntityFrameworkCore;
using Teams.Data.Filters;
using Teams.Domain.Entities;
using Teams.Domain.Enums;
using Teams.Data.Context;
using Teams.Data.Models;

namespace Teams.Data.Repositories.Jobs;

/// <summary>A read-only repository containing instances of <see cref="Job"/>.</summary>
public class ReadOnlyJobsRepository(ApiDbContext context) : RepositoryBase(context), IReadOnlyJobsRepository
{
    /// <inheritdoc />
    public async Task<Job?> GetByIdAsync(string id, CancellationToken cancellationToken)
        => await Context.Jobs
            .SingleOrDefaultAsync(entity => entity.Id == id, cancellationToken);

    /// <inheritdoc />
    public async Task<Job?> GetByIdempotencyKeyAsync(string idempotencyKey, CancellationToken cancellationToken)
        => await Context.Jobs
            .SingleOrDefaultAsync(entity => entity.IdempotencyKey == idempotencyKey, cancellationToken);

    /// <inheritdoc />
    public async Task<IEnumerable<Job>> GetAsync(
        JobTypeEnum? type = null,
        JobStatusEnum? status = null,
        string? errorCode = null,
        DateFilter? dateFilter = null,
        PaginationFilter? pagination = null,
        CancellationToken cancellationToken = default)
        => await Context.Jobs
            .ApplyTypeFilter(type)
            .ApplyStatusFilter(status)
            .ApplyErrorCodeFilter(errorCode)
            .ApplyCreatedFromFilter(dateFilter?.CreatedFrom)
            .ApplyCreatedToFilter(dateFilter?.CreatedTo)
            .ApplyModifiedFromFilter(dateFilter?.ModifiedFrom)
            .ApplyModifiedToFilter(dateFilter?.ModifiedTo)
            .ApplyCursor(pagination?.Cursor)
            .ApplyPagination(pagination?.PageSize ?? Constants.DefaultPageSize)
            .ToListAsync(cancellationToken);
}