using Microsoft.EntityFrameworkCore;
using Teams.Data.Context;
using Teams.Data.Filters;
using Teams.Data.Models;
using Teams.Domain.Entities;
using Teams.Domain.Enums;

namespace Teams.Data.Repositories.Invitations;

public class ReadOnlyInvitationsRepository(ApiDbContext context) : RepositoryBase(context), IReadOnlyInvitationsRepository
{
    public async Task<Invitation?> GetByIdAsync(string id, CancellationToken cancellationToken) =>
        await Context.Invitations
            .Include(invitation => invitation.Game)
                .ThenInclude(game => game.Organiser)
            .Include(invitation => invitation.User)
            .SingleOrDefaultAsync(entity => entity.Id == id, cancellationToken);

    public async Task<IEnumerable<Invitation>> GetInvitationsAsync(
        string? gameId = null,
        string? userId = null,
        string? emailAddress = null,
        InvitationStatusEnum? status = null,
        DateFilter? dateFilter = null,
        PaginationFilter? pagination = null,
        CancellationToken cancellationToken = default) =>
        await Context.Invitations
            .Include(invitation => invitation.Game)
                .ThenInclude(game => game.Organiser)
            .Include(invitation => invitation.User)
            .ApplyGameIdFilter(gameId)
            .ApplyUserIdFilter(userId)
            .ApplyEmailAddressFilter(emailAddress)
            .ApplyStatusFilter(status)
            .ApplyBaseEntityDateFilters(dateFilter)
            .ApplyCursor(pagination?.Cursor)
            .ApplyPagination(pagination?.PageSize ?? Constants.DefaultPageSize)
            .ToListAsync(cancellationToken);

}