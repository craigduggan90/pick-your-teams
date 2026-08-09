using Teams.Data.Models;
using Teams.Domain.Entities;
using Teams.Domain.Enums;

namespace Teams.Data.Repositories.Invitations;

public interface IReadOnlyInvitationsRepository : IReadOnlyRepository<Invitation>
{
    Task<IEnumerable<Invitation>> GetInvitationsAsync(
        string? gameId = null,
        string? userId = null,
        string? emailAddress = null,
        InvitationStatusEnum? status = null,
        DateFilter? dateFilter = null,
        PaginationFilter? pagination = null,
        CancellationToken cancellationToken = default);
}