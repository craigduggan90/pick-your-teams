using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Teams.Core.Exceptions;

namespace Teams.Api.Infrastructure.Errors.Handlers;

public class NotFoundExceptionHandler : IExceptionHandler
{
    private const string Title = "Resource Not Found";
    private const string Type = $"{Constants.ErrorUrlBase}/not-found";
    internal const int StatusCode = 404;

    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        if (exception is not NotFoundException notFoundException)
            return false;

        httpContext.Response.StatusCode = StatusCode;
        await httpContext.Response.WriteAsJsonAsync(GetProblemDetails(notFoundException), cancellationToken);
        return true;
    }

    internal static ProblemDetails GetProblemDetails(NotFoundException exception)
    {
        var problemDetails = new ProblemDetails
        {
            Title = Title,
            Detail = exception.Message,
            Type = Type,
            Status = StatusCode
        };

        if (exception.ResourceType is not null)
            problemDetails.Extensions.Add("resource", exception.ResourceType);

        if (exception.ResourceIdentifier is not null)
            problemDetails.Extensions.Add("identifier", exception.ResourceIdentifier);

        return problemDetails;
    }
}