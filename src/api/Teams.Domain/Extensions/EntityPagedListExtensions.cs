using Teams.Common.Pagination;
using Teams.Domain.Interfaces;

namespace Teams.Domain.Extensions;

public static class EntityPagedListExtensions
{
    public static PagedList<TEntity> ToPagedList<TEntity>(this IReadOnlyCollection<TEntity> collection)
        where TEntity : IHasCursor
    {
        return new PagedList<TEntity>(collection, collection.GetCursor(), collection.Count);
    }

    public static PagedList<TModel> ToPagedList<TEntity, TModel>(
        this IReadOnlyCollection<TEntity> collection,
        Func<TEntity, TModel> mutator)
        where TEntity : IHasCursor
    {
        var mapped = collection.Select(mutator).ToList();
        return new PagedList<TModel>(mapped, collection.GetCursor(), collection.Count);
    }

    private static string? GetCursor<T>(this IReadOnlyCollection<T> entities)
        where T : IHasCursor
    {
        if (entities is not { Count: > 0 })
            return null;

        return (entities.MaxBy(e => e.Cursor)?.Cursor).TryEncodeCursor(out var cursor)
            ? cursor
            : null;
    }
}