using FluentValidation;
using Microsoft.Extensions.Logging;
using Teams.Core.CQRS;
using Teams.Core.Exceptions;
using Teams.Data.Services;
using Teams.Domain.Entities;

namespace Teams.Core.UseCases.Users.CreateUser;

public class CreateUserCommandHandler(
    IUnitOfWork uow,
    IValidator<CreateUserCommand> validator,
    ILogger<CreateUserCommandHandler> logger)
    : IRequestHandler<CreateUserCommand, User>
{
    public async Task<User> HandleAsync(CreateUserCommand request, CancellationToken cancellationToken)
    {
        CommandValidationException.ThrowIfValidationFailed(await validator.ValidateAsync(request, cancellationToken));

        if (await uow.Users.GetByEmailAddressAsync(request.Email, cancellationToken) is not null)
        {
            throw CommandValidationException.ForEmailConflict();
        }

        var user = await uow.Users.CreateAsync(new User(
            request.DisplayName,
            request.ExternalId,
            request.Email,
            request.Mobile), cancellationToken);

        await uow.SaveChangesAsync(cancellationToken);
        logger.LogInformation("User created: {user}", user);

        return user;
    }
}