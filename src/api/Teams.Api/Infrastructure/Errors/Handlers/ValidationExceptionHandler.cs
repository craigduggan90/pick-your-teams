using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Teams.Core.Exceptions;

namespace Teams.Api.Infrastructure.Errors.Handlers;

public class ValidationExceptionHandler : IExceptionHandler
{
    private const string Title = "Validation Error";
    private const string Detail = "One or more validation failures occurred.";
    private const string Type = $"{Constants.ErrorUrlBase}/validation";
    internal const int CommandValidationStatusCode = 422;
    internal const int QueryValidationStatusCode = 400;

    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        if (exception is not ValidationExceptionBase baseException)
            return false;

        // CommandValidation returns 422, QueryValidation returns 400
        var statusCode = exception is CommandValidationException
            ? CommandValidationStatusCode
            : QueryValidationStatusCode;

        var content = GetProblemDetails(statusCode, baseException);
        httpContext.Response.StatusCode = statusCode;
        await httpContext.Response.WriteAsJsonAsync(content, cancellationToken);
        return true;
    }

    internal static ProblemDetails GetProblemDetails(int statusCode, ValidationExceptionBase exception) =>
        new()
        {
            Title = Title,
            Detail = Detail,
            Type = Type,
            Status = statusCode,
            Extensions = new Dictionary<string, object?>
            {
                {
                    "errors", exception.Errors
                        .GroupBy(e => e.PropertyName)
                        .ToDictionary(
                            keySelector: kvp => kvp.Key,
                            elementSelector: kvp => kvp.Select(item => item.ErrorMessage))
                }
            }
        };
}