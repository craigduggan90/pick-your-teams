using Teams.Domain.Entities;
using Teams.Domain.Enums;
using Teams.Data.Models;

namespace Teams.Data.Repositories.Jobs;

/// <summary>Describes a read-only repository containing instances of <see cref="Job"/>.</summary>
public interface IReadOnlyJobsRepository : IReadOnlyRepository<Job>
{
    /// <summary>Get a job by its idempotency key.</summary>
    /// <param name="idempotencyKey">The idempotency key associated with the job.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    Task<Job?> GetByIdempotencyKeyAsync(string idempotencyKey, CancellationToken cancellationToken);

    /// <summary>Get a collection of jobs with optional filters applied.</summary>
    /// <param name="type">Limit results to records matching this type.</param>
    /// <param name="status">Limit results to records with this status.</param>
    /// <param name="errorCode">Limit results to records with this error code.</param>
    /// <param name="dateFilter">Limit results to records matching this date filter.</param>
    /// <param name="pagination">Limit results to a page matching this pagination filter.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    Task<IEnumerable<Job>> GetAsync(
        JobTypeEnum? type = null,
        JobStatusEnum? status = null,
        string? errorCode = null,
        DateFilter? dateFilter = null,
        PaginationFilter? pagination = null,
        CancellationToken cancellationToken = default);
}