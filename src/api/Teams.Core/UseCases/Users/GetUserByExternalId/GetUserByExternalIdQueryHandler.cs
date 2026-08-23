using Teams.Core.CQRS;
using Teams.Core.Exceptions;
using Teams.Data.Repositories.Users;
using Teams.Domain.Entities;

namespace Teams.Core.UseCases.Users.GetUserByExternalId;

public class GetUserByExternalIdQueryHandler(IReadOnlyUsersRepository repository)
    : IRequestHandler<GetUserByExternalIdQuery, User>
{
    public async Task<User> HandleAsync(GetUserByExternalIdQuery request, CancellationToken cancellationToken) =>
        await repository.GetByExternalIdAsync(request.ExternalId, cancellationToken)
        ?? throw new NotFoundException(typeof(User), request.ExternalId);
}