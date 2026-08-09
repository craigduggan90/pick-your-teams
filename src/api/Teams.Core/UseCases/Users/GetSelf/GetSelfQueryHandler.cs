using Teams.Core.CQRS;
using Teams.Core.Exceptions;
using Teams.Core.Services;
using Teams.Data.Repositories.Users;
using Teams.Domain.Entities;

namespace Teams.Core.UseCases.Users.GetSelf;

public class GetSelfQueryHandler(IReadOnlyUsersRepository repository, IActorAccessor actor)
    : IRequestHandler<GetSelfQuery, User>
{
    public async Task<User> HandleAsync(GetSelfQuery request, CancellationToken cancellationToken) =>
        await repository.GetByIdAsync(actor.Current.Id, cancellationToken)
        ?? throw new NotFoundException(typeof(User), actor.Current.Id);
}