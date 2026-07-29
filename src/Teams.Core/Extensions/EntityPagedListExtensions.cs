using Teams.Common.Pagination;
using Teams.Domain.Interfaces;

namespace Teams.Core.Extensions;

internal static class EntityPagedListExtensions
{
    public static PagedList<TModel> ToPagedList<TEntity, TModel>(this IReadOnlyList<TEntity> list,
        Func<TEntity, TModel> converter) where TEntity : IHasCursor =>
        new(list.Select(converter).ToList(), GetCursor(list), list.Count);

    private static string? GetCursor<T>(IReadOnlyList<T> entities)
        where T : IHasCursor
    {
        if (entities is not { Count: > 0 })
            return null;

        return (entities.MaxBy(e => e.Cursor)?.Cursor).TryEncodeCursor(out var cursor)
            ? cursor
            : null;
    }
}