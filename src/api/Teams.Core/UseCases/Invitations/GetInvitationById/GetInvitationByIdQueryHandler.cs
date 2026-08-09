using Teams.Core.CQRS;
using Teams.Core.Exceptions;
using Teams.Core.Services;
using Teams.Data.Repositories.Invitations;
using Teams.Domain.Entities;

namespace Teams.Core.UseCases.Invitations.GetInvitationById;

public class GetInvitationByIdQueryHandler(IReadOnlyInvitationsRepository repository, IActorAccessor actor)
    : IRequestHandler<GetInvitationByIdQuery, Invitation>
{
    public async Task<Invitation> HandleAsync(GetInvitationByIdQuery request, CancellationToken cancellationToken)
    {
        var invitation = await repository.GetByIdAsync(request.Id, cancellationToken) ??
               throw new NotFoundException(typeof(Invitation), request.Id);

        actor.Current.ThrowIfNotOrganiserOrUser(invitation.UserId ?? string.Empty, invitation.Game.OrganiserId);

        return invitation;
    }
}