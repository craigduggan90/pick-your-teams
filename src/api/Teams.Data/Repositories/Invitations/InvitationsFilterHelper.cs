using Teams.Domain.Entities;
using Teams.Domain.Enums;

namespace Teams.Data.Repositories.Invitations;

public static class InvitationsFilterHelper
{
    public static IQueryable<Invitation> ApplyEmailAddressFilter(this IQueryable<Invitation> queryable, string? value)
        => value is null
            ? queryable
            : queryable.Where(entity => entity.EmailAddress.Contains(value));

    public static IQueryable<Invitation> ApplyGameIdFilter(this IQueryable<Invitation> queryable, string? value)
        => value is null
            ? queryable
            : queryable.Where(entity => entity.GameId == value);

    public static IQueryable<Invitation> ApplyUserIdFilter(this IQueryable<Invitation> queryable, string? value)
        => value is null
            ? queryable
            : queryable.Where(entity => entity.UserId == value);

    public static IQueryable<Invitation> ApplyStatusFilter(this IQueryable<Invitation> queryable, InvitationStatusEnum? value)
        => value is null
            ? queryable
            : queryable.Where(entity => entity.Status == value);
}