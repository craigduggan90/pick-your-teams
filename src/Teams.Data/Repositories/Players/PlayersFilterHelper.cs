using Teams.Domain.Entities;

namespace Teams.Data.Repositories.Players;

/// <summary>Filter helper for <see cref="Player"/> queries.</summary>
public static class PlayersFilterHelper
{
    /// <summary>Filters a collection of <see cref="Player"/> objects by type.</summary>
    /// <param name="queryable">The collection to filter.</param>
    /// <param name="value">The value to filter by.</param>
    /// <returns>A reference to the queryable after the filter operation.</returns>
    public static IQueryable<Player> ApplyNameFilter(this IQueryable<Player> queryable, string? value)
        => value is null
            ? queryable
            : queryable.Where(player => player.Name.Contains(value));

    /// <summary>Filters a collection of <see cref="Player"/> objects by error code.</summary>
    /// <param name="queryable">The collection to filter.</param>
    /// <param name="value">The value to filter by.</param>
    /// <returns>A reference to the queryable after the filter operation.</returns>
    public static IQueryable<Player> ApplyRatingFromFilter(this IQueryable<Player> queryable, int? value)
        => value is null
            ? queryable
            : queryable.Where(player => player.Rating >= value);

    /// <summary>Filters a collection of <see cref="Player"/> objects by error code.</summary>
    /// <param name="queryable">The collection to filter.</param>
    /// <param name="value">The value to filter by.</param>
    /// <returns>A reference to the queryable after the filter operation.</returns>
    public static IQueryable<Player> ApplyRatingToFilter(this IQueryable<Player> queryable, int? value)
        => value is null
            ? queryable
            : queryable.Where(player => player.Rating < value);
}