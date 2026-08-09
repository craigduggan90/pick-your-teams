using FluentValidation;
using Microsoft.Extensions.Logging;
using Teams.Core.CQRS;
using Teams.Core.Exceptions;
using Teams.Core.Services;
using Teams.Data.Services;
using Teams.Domain.Entities;

namespace Teams.Core.UseCases.Users.UpdateUser;

public class UpdateUserCommandHandler(
    IUnitOfWork uow,
    IActorAccessor actor,
    IValidator<UpdateUserCommand> validator,
    ILogger<UpdateUserCommandHandler> logger)
    : IRequestHandler<UpdateUserCommand, User>
{
    public async Task<User> HandleAsync(UpdateUserCommand request, CancellationToken cancellationToken)
    {
        actor.Current.ThrowIfNotUser(request.Id);

        CommandValidationException.ThrowIfValidationFailed(await validator.ValidateAsync(request, cancellationToken));

        var user = await uow.Users.GetByIdAsync(request.Id, cancellationToken)
                   ?? throw new NotFoundException(typeof(User), request.Id);

        if (request.Tag is not null)
        {
            // If the tag is in use and associated with a different user, throw an exception
            if (await uow.Users.GetByTagAsync(request.Tag, cancellationToken) is { } tag &&
                !tag.Id.Equals(user.Id, StringComparison.OrdinalIgnoreCase))
            {
                throw CommandValidationException.ForTagConflict();
            }
        }

        user.Update(request.Tag, request.DisplayName, request.Email, request.Mobile);
        if (!user.IsDirty)
            return user;

        await uow.Users.UpdateAsync(user, cancellationToken);
        await uow.SaveChangesAsync(cancellationToken);
        logger.LogInformation("User updated: {user}", user);

        return user;
    }
}