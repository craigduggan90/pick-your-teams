using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace Teams.Api.Infrastructure.Errors.Handlers;

// <summary>Handler for exceptions that are not caught by other implementations of IExceptionHandler.</summary>
/// <param name="logger">The logger.</param>
/// <remarks>
/// This must be the final exception handler added to the service container.
/// </remarks>
public class UnhandledExceptionHandler(ILogger<UnhandledExceptionHandler> logger) : IExceptionHandler
{
    internal const int StatusCode = 500;

    /// <inheritdoc />
    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        // If this handler is generating the response, then it is an unhandled error that we should log
        logger.LogError(exception, "An unexpected error has occurred.");

        // Then we generate the problem details and shape the response.
        var content = GetProblemDetails();
        httpContext.Response.StatusCode = StatusCode;
        await httpContext.Response.WriteAsJsonAsync(content, cancellationToken);
        return true;
    }

    /// <summary>Get the <see cref="ProblemDetails"/> object representing an unhandled exception.</summary>
    internal static ProblemDetails GetProblemDetails()
        => new()
        {
            Title = "Unexpected Error",
            Detail = "An unexpected error has occurred.",
            Type = "https://www.Teams.api/errors/unhandled",
            Status = StatusCode
        };
}