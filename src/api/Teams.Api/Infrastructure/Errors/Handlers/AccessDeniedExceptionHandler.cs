using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Teams.Core.Exceptions;

namespace Teams.Api.Infrastructure.Errors.Handlers;

public class AccessDeniedExceptionHandler : IExceptionHandler
{
    private const string Title = "Forbidden";
    private const string Type = $"{Constants.ErrorUrlBase}/forbidden";
    internal const int StatusCode = 403;

    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        if (exception is not AccessDeniedException typedException)
            return false;

        httpContext.Response.StatusCode = StatusCode;
        await httpContext.Response.WriteAsJsonAsync(GetProblemDetails(typedException), cancellationToken);
        return true;
    }

    internal static ProblemDetails GetProblemDetails(AccessDeniedException exception) =>
        new()
        {
            Title = Title,
            Detail = exception.Message,
            Type = Type,
            Status = StatusCode
        };
}