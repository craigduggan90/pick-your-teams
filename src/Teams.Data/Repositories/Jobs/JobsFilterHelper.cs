using Teams.Domain.Entities;
using Teams.Domain.Enums;

namespace Teams.Data.Repositories.Jobs;

/// <summary>Filter helper for <see cref="Job"/> queries.</summary>
public static class JobsFilterHelper
{
    /// <summary>Filters a collection of <see cref="Job"/> objects by type.</summary>
    /// <param name="queryable">The collection to filter.</param>
    /// <param name="value">The value to filter by.</param>
    /// <returns>A reference to the queryable after the filter operation.</returns>
    public static IQueryable<Job> ApplyTypeFilter(this IQueryable<Job> queryable, JobTypeEnum? value)
        => value is null
            ? queryable
            : queryable.Where(job => job.Type == value);

    /// <summary>Filters a collection of <see cref="Job"/> objects by status.</summary>
    /// <param name="queryable">The collection to filter.</param>
    /// <param name="value">The value to filter by.</param>
    /// <returns>A reference to the queryable after the filter operation.</returns>
    public static IQueryable<Job> ApplyStatusFilter(this IQueryable<Job> queryable, JobStatusEnum? value)
        => value is null
            ? queryable
            : queryable.Where(job => job.Status == value);

    /// <summary>Filters a collection of <see cref="Job"/> objects by error code.</summary>
    /// <param name="queryable">The collection to filter.</param>
    /// <param name="value">The value to filter by.</param>
    /// <returns>A reference to the queryable after the filter operation.</returns>
    public static IQueryable<Job> ApplyErrorCodeFilter(this IQueryable<Job> queryable, string? value)
        => value is null
            ? queryable
            : queryable.Where(job => job.ErrorCode == value);
}