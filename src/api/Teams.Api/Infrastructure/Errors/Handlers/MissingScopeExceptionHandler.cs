using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Teams.Api.Exceptions;

namespace Teams.Api.Infrastructure.Errors.Handlers;

public class MissingScopeExceptionHandler : IExceptionHandler
{
    private const string Title = "Forbidden";
    private const string Type = $"{Constants.ErrorUrlBase}/missing-scope";
    internal const int StatusCode = 403;

    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        if (exception is not MissingScopeException typedException)
            return false;

        httpContext.Response.StatusCode = StatusCode;
        await httpContext.Response.WriteAsJsonAsync(GetProblemDetails(typedException), cancellationToken);
        return true;
    }

    internal static ProblemDetails GetProblemDetails(MissingScopeException exception) =>
        new()
        {
            Title = Title,
            Detail = exception.Message,
            Type = Type,
            Status = StatusCode
        };
}