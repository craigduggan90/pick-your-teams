using Teams.Domain.Interfaces;

namespace Teams.Data.Filters;

/// <summary>Filter helper for queries involving entities implementing <see cref="IHasCreatedTimestamp"/>.</summary>
public static class HasCreatedTimestampQueryFilter
{
    /// <summary>Filters a collection of <typeparamref name="T"/> objects by minimum creation date (inclusive).</summary>
    /// <param name="queryable">The collection to filter.</param>
    /// <param name="value">The value to filter by.</param>
    /// <typeparam name="T">The implementation of <see cref="IHasCreatedTimestamp"/> in the queryable.</typeparam>
    /// <returns>A reference to the queryable after the filter operation.</returns>
    public static IQueryable<T> ApplyCreatedFromFilter<T>(this IQueryable<T> queryable, DateTime? value)
        where T : IHasCreatedTimestamp
        => value == null
            ? queryable
            : queryable.Where(instance => instance.DateCreated >= value.Value);

    /// <summary>Filters a collection of <typeparamref name="T"/> objects by maximum creation date (exclusive).</summary>
    /// <param name="queryable">The collection to filter.</param>
    /// <param name="value">The value to filter by.</param>
    /// <typeparam name="T">The implementation of <see cref="IHasCreatedTimestamp"/> in the queryable.</typeparam>
    /// <returns>A reference to the queryable after the filter operation.</returns>
    public static IQueryable<T> ApplyCreatedToFilter<T>(this IQueryable<T> queryable, DateTime? value)
        where T : IHasCreatedTimestamp
        => value == null
            ? queryable
            : queryable.Where(instance => instance.DateCreated < value.Value);
}