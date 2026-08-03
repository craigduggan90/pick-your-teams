using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Teams.Core.Exceptions;

namespace Teams.Api.Infrastructure.Errors.Handlers;

public class ConcurrencyTokenMismatchExceptionHandler : IExceptionHandler
{
    private const string Title = "Precondition Failed";
    private const string Type = "https://www.Teams.api/errors/concurrency";
    internal const int StatusCode = 412;

    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        if (exception is not ConcurrencyTokenMismatchException typedException)
            return false;

        httpContext.Response.StatusCode = StatusCode;
        await httpContext.Response.WriteAsJsonAsync(GetProblemDetails(typedException), cancellationToken);
        return true;
    }

    internal static ProblemDetails GetProblemDetails(ConcurrencyTokenMismatchException exception) =>
        new()
        {
            Title = Title,
            Detail = exception.Message,
            Type = Type,
            Status = StatusCode
        };
}