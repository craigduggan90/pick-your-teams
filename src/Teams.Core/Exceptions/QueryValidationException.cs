using FluentValidation.Results;

namespace Teams.Core.Exceptions;

public class QueryValidationException(IEnumerable<ValidationFailure> errors)
    : ValidationExceptionBase(errors)
{
    public static void ThrowIfValidationFailed(ValidationResult result)
    {
        if (result.IsValid)
            return;

        throw new QueryValidationException(result.Errors);
    }
}