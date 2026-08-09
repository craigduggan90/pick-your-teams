using Teams.Core.CQRS;
using Teams.Domain.Entities;
using Teams.Domain.Enums;

namespace Teams.Core.UseCases.Invitations.GetInvitations;

public record GetInvitationsQuery(
    string? GameId,
    string? UserId,
    string? EmailAddress,
    InvitationStatusEnum? Status,
    DateTime? CreatedFrom,
    DateTime? CreatedTo,
    DateTime? ModifiedFrom,
    DateTime? ModifiedTo,
    int? PageSize,
    long? Cursor)
    : IRequest<IReadOnlyCollection<Invitation>>;