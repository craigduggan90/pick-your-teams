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
}