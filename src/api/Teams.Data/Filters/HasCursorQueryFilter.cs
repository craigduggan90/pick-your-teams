using Teams.Domain.Interfaces;

namespace Teams.Data.Filters;

/// <summary>Filter helper for queries involving entities implementing <see cref="IHasCursor"/>.</summary>
public static class HasCursorQueryFilter
{
    /// <summary>Applies a cursor offset to the collection.</summary>
    /// <param name="queryable">The collection to limit.</param>
    /// <param name="value">The maximum cursor value of the last result set.</param>
    /// <typeparam name="T">The implementation of <see cref="IHasCursor"/> in the queryable.</typeparam>
    /// <returns>A reference to the queryable after the filter operation.</returns>
    public static IQueryable<T> ApplyCursor<T>(this IQueryable<T> queryable, long? value)
        where T : IHasCursor
        => value is null ? queryable : queryable.Where(entity => entity.Cursor > value);

    /// <summary>Applies a cursor offset to the collection.</summary>
    /// <param name="queryable">The collection to limit.</param>
    /// <param name="pageSize">The number of items to include in the page.</param>
    /// <typeparam name="T">The implementation of <see cref="IHasCursor"/> in the queryable.</typeparam>
    /// <returns>A reference to the collection after the filter operation.</returns>
    /// <remarks>This should always be the final step in the query construction.</remarks>
    public static IQueryable<T> ApplyPagination<T>(this IQueryable<T> queryable, int? pageSize)
        where T : IHasCursor
        => queryable.OrderBy(entity => entity.Cursor)
            .Take(pageSize ?? Constants.DefaultPageSize);
}