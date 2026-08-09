using Teams.Core.CQRS;
using Teams.Domain.Entities;

namespace Teams.Core.UseCases.Invitations.AcceptInvitation;

public record AcceptInvitationCommand(string Id) : IRequest<Invitation>;