using Teams.Core.CQRS;
using Teams.Domain.Entities;

namespace Teams.Core.UseCases.Invitations.DeclineInvitation;

public record DeclineInvitationCommand(string Id) : IRequest<Invitation>;