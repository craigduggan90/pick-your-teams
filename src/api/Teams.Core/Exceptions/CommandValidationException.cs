using FluentValidation.Results;
using Teams.Domain.Entities;

namespace Teams.Core.Exceptions;

public class CommandValidationException(IEnumerable<ValidationFailure> errors)
    : ValidationExceptionBase(errors)
{
    public static void ThrowIfValidationFailed(ValidationResult result)
    {
        if (result.IsValid)
            return;

        throw new CommandValidationException(result.Errors);
    }

    public static CommandValidationException ForTagConflict() =>
        new([new ValidationFailure(nameof(User.Tag), "Tag not available.")]);

    // "Email", not nameof(User.EmailAddress) - this needs to match the command/request property
    // name (CreateUserCommand.Email, UpdateUserCommand.Email) so it lands under the same key as
    // this same field's other validation errors, not the differently-named domain property.
    public static CommandValidationException ForEmailConflict() =>
        new([new ValidationFailure("Email", "Email address already in use.")]);
}