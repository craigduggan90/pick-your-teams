using Teams.Data.Models;
using Teams.Domain.Entities;
using Teams.Domain.Enums;

namespace Teams.Data.Repositories.Players;

public static class PlayersFilterHelper
{
    public static IQueryable<Player> ApplyGameIdFilter(this IQueryable<Player> queryable, string? value) =>
        value is null
            ? queryable
            : queryable.Where(entity => entity.GameId == value);

    public static IQueryable<Player> ApplyDisplayNameFilter(this IQueryable<Player> queryable, string? value) =>
        value is null
            ? queryable
            : queryable.Where(entity => entity.DisplayName.Contains(value));

    public static IQueryable<Player> ApplyUserIdFilter(this IQueryable<Player> queryable, string? value) =>
        value is null
            ? queryable
            : queryable.Where(entity => entity.UserId == value);

    public static IQueryable<Player> ApplyTeamFilter(this IQueryable<Player> queryable, GameTeamEnum? value) =>
        value is null
            ? queryable
            : queryable.Where(entity => entity.Team == value);

    public static IQueryable<Player> ApplyTypeFilter(this IQueryable<Player> queryable, PlayerTypeEnum? value) =>
        value is null
            ? queryable
            : queryable.Where(entity => entity.Type == value);

    public static IQueryable<Player> ApplyRatingFilter(this IQueryable<Player> queryable, RangeFilter<int>? value) =>
        value is null
            ? queryable
            : queryable.ApplyRatingFromFilter(value.From).ApplyRatingToFilter(value.To);

    private static IQueryable<Player> ApplyRatingFromFilter(this IQueryable<Player> queryable, int? value) =>
        value is null
            ? queryable
            : queryable.Where(entity => entity.Rating >= value);

    private static IQueryable<Player> ApplyRatingToFilter(this IQueryable<Player> queryable, int? value) =>
        value is null
            ? queryable
            : queryable.Where(entity => entity.Rating < value);
}