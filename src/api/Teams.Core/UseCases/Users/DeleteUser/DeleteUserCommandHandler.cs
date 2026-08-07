using Microsoft.Extensions.Logging;
using Teams.Core.CQRS;
using Teams.Core.Exceptions;
using Teams.Core.Services;
using Teams.Data.Services;
using Teams.Domain.Entities;

namespace Teams.Core.UseCases.Users.DeleteUser;

public class DeleteUserCommandHandler(
    IUnitOfWork uow,
    IActorAccessor actor,
    ILogger<DeleteUserCommandHandler> logger)
    : IRequestHandler<DeleteUserCommand, User>
{
    public async Task<User> HandleAsync(DeleteUserCommand request, CancellationToken cancellationToken)
    {
        actor.Current.ThrowIfNotUser(request.Id);

        var user = await uow.Users.GetByIdAsync(request.Id, cancellationToken)
                   ?? throw new NotFoundException(typeof(User), request.Id);

        user.Delete();

        await uow.Users.UpdateAsync(user, cancellationToken);
        await uow.SaveChangesAsync(cancellationToken);
        logger.LogInformation("User deleted: {user}", user);

        return user;
    }
}