using Teams.Core.CQRS;
using Teams.Core.Exceptions;
using Teams.Data.Repositories.Invitations;
using Teams.Domain.Entities;

namespace Teams.Core.UseCases.Invitations.GetInvitationById;

public class GetInvitationByIdQueryHandler(IReadOnlyInvitationsRepository repository)
    : IRequestHandler<GetInvitationByIdQuery, Invitation>
{
    public async Task<Invitation> HandleAsync(GetInvitationByIdQuery request, CancellationToken cancellationToken) =>
        await repository.GetByIdAsync(request.Id, cancellationToken) ??
        throw new NotFoundException(typeof(Invitation), request.Id);
}