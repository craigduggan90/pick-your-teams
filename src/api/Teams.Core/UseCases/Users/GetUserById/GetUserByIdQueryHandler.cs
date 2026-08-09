using Teams.Core.CQRS;
using Teams.Core.Exceptions;
using Teams.Data.Repositories.Users;
using Teams.Domain.Entities;

namespace Teams.Core.UseCases.Users.GetUserById;

public class GetUserByIdQueryHandler(IReadOnlyUsersRepository repository)
    : IRequestHandler<GetUserByIdQuery, User>
{
    public async Task<User> HandleAsync(GetUserByIdQuery request, CancellationToken cancellationToken) =>
        await repository.GetByIdAsync(request.Id, cancellationToken)
        ?? throw new NotFoundException(typeof(User), request.Id);
}