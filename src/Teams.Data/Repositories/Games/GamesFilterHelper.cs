using Teams.Domain.Entities;

namespace Teams.Data.Repositories.Games;

/// <summary>Filter helper for <see cref="Game"/> queries.</summary>
public static class GamesFilterHelper
{
    public static IQueryable<Game> ApplyLocationFilter(this IQueryable<Game> queryable, string? value)
        => value is null
            ? queryable
            : queryable.Where(game => game.Location != null && game.Location.Contains(value));

    public static IQueryable<Game> ApplyStartTimeFromFilter(this IQueryable<Game> queryable, DateTime? value)
        => value is null
            ? queryable
            : queryable.Where(game => game.StartTime <= value);

    public static IQueryable<Game> ApplyStartTimeToFilter(this IQueryable<Game> queryable, DateTime? value)
        => value is null
            ? queryable
            : queryable.Where(game => game.StartTime > value);

    public static IQueryable<Game> ApplyDurationFromFilter(this IQueryable<Game> queryable, int? value)
        => value is null
            ? queryable
            : queryable.Where(game => game.Duration <= value);

    public static IQueryable<Game> ApplyDurationToFilter(this IQueryable<Game> queryable, int? value)
        => value is null
            ? queryable
            : queryable.Where(game => game.Duration > value);

    public static IQueryable<Game> ApplyTeamSizeFilter(this IQueryable<Game> queryable, int? value)
        => value is null
            ? queryable
            : queryable.Where(game => game.TeamSize == value);
}