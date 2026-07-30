using FluentValidation.Results;

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
}