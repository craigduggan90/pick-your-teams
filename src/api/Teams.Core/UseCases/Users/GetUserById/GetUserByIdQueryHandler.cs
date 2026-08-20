using Teams.Core.CQRS;
using Teams.Core.Exceptions;
using Teams.Data.Repositories.Invitations;
using Teams.Data.Repositories.Users;
using Teams.Domain.Entities;
using Teams.Domain.Enums;

namespace Teams.Core.UseCases.Users.GetUserById;

public class GetUserByIdQueryHandler(
    IReadOnlyUsersRepository repository,
    IReadOnlyInvitationsRepository invitationsRepository)
    : IRequestHandler<GetUserByIdQuery, UserDetail>
{
    public async Task<UserDetail> HandleAsync(GetUserByIdQuery request, CancellationToken cancellationToken)
    {
        var user = await repository.GetByIdAsync(request.Id, cancellationToken)
            ?? throw new NotFoundException(typeof(User), request.Id);

        var pendingInvitations = await invitationsRepository.CountInvitationsAsync(
            user.Id, InvitationStatusEnum.Open, cancellationToken);

        return new UserDetail(user, pendingInvitations);
    }
}