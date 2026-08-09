using Teams.Core.CQRS;
using Teams.Domain.Entities;

namespace Teams.Core.UseCases.Invitations.GetInvitationById;

public record GetInvitationByIdQuery(string Id) : IRequest<Invitation>;