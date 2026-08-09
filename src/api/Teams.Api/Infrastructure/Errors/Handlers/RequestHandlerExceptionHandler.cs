using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Teams.Core.Exceptions;

namespace Teams.Api.Infrastructure.Errors.Handlers;

public class RequestHandlerExceptionHandler : IExceptionHandler
{
    private const string Type = $"{Constants.ErrorUrlBase}/service";

    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        if (exception is not RequestHandlerException requestHandlerException)
            return false;

        httpContext.Response.StatusCode = requestHandlerException.StatusCode;
        await httpContext.Response.WriteAsJsonAsync(GetProblemDetails(requestHandlerException), cancellationToken);
        return true;
    }

    internal static ProblemDetails GetProblemDetails(RequestHandlerException exception)
    {
        var problemDetails = new ProblemDetails
        {
            Title = "Service Error",
            Detail = exception.Message,
            Type = Type,
            Status = exception.StatusCode
        };

        return problemDetails;
    }

}