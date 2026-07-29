using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Teams.Api.Exceptions;

namespace Teams.Api.Infrastructure.Errors.Handlers;

public class MissingHeaderExceptionHandler : IExceptionHandler
{
    private const string Title = "Precondition Required";
    private const string Type = "https://www.Teams.api/errors/missing-header";
    internal const int StatusCode = 428;

    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        if (exception is not MissingHeaderException typedException)
            return false;

        httpContext.Response.StatusCode = StatusCode;
        await httpContext.Response.WriteAsJsonAsync(GetProblemDetails(typedException), cancellationToken);
        return true;
    }

    internal static ProblemDetails GetProblemDetails(MissingHeaderException exception) =>
        new()
        {
            Title = Title,
            Detail = exception.Message,
            Type = Type,
            Status = StatusCode
        };
}