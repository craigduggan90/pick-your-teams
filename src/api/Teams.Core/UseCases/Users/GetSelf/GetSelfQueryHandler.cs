using Teams.Core.CQRS;
using Teams.Core.Exceptions;
using Teams.Core.Services;
using Teams.Data.Repositories.Invitations;
using Teams.Data.Repositories.Users;
using Teams.Domain.Entities;
using Teams.Domain.Enums;

namespace Teams.Core.UseCases.Users.GetSelf;

public class GetSelfQueryHandler(
    IReadOnlyUsersRepository repository,
    IReadOnlyInvitationsRepository invitationsRepository,
    IActorAccessor actor)
    : IRequestHandler<GetSelfQuery, UserDetail>
{
    public async Task<UserDetail> HandleAsync(GetSelfQuery request, CancellationToken cancellationToken)
    {
        var user = await repository.GetByIdAsync(actor.Current.Id, cancellationToken)
            ?? throw new NotFoundException(typeof(User), actor.Current.Id);

        var pendingInvitations = await invitationsRepository.CountInvitationsAsync(
            user.Id, InvitationStatusEnum.Open, cancellationToken);

        return new UserDetail(user, pendingInvitations);
    }
}
