using Teams.Data.Models;
using Teams.Domain.Entities;

namespace Teams.Data.Repositories.Users;

public static class UsersFilterHelper
{
    public static IQueryable<User> ApplyEmailAddressFilter(this IQueryable<User> queryable, string? value) =>
        value is null
            ? queryable
            : queryable.Where(entity => entity.EmailAddress.Contains(value));

    public static IQueryable<User> ApplyTagFilter(this IQueryable<User> queryable, string? value) =>
        value is null
            ? queryable
            : queryable.Where(entity => entity.Tag.Contains(value));

    public static IQueryable<User> ApplyDisplayNameFilter(this IQueryable<User> queryable, string? value) =>
        value is null
            ? queryable
            : queryable.Where(entity => entity.DisplayName.Contains(value));

    public static IQueryable<User> ApplyRatingFilter(this IQueryable<User> queryable, RangeFilter<int>? value) =>
        value is null
            ? queryable
            : queryable.ApplyRatingFromFilter(value.From).ApplyRatingToFilter(value.To);

    private static IQueryable<User> ApplyRatingFromFilter(this IQueryable<User> queryable, int? value) =>
        value is null
            ? queryable
            : queryable.Where(entity => entity.Rating >= value);

    private static IQueryable<User> ApplyRatingToFilter(this IQueryable<User> queryable, int? value) =>
        value is null
            ? queryable
            : queryable.Where(entity => entity.Rating < value);
}