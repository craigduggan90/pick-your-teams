using Microsoft.AspNetCore.Mvc;
using System.Diagnostics.CodeAnalysis;

namespace Teams.Api.Infrastructure.Swagger.Examples.V1.Common;

[ExcludeFromCodeCoverage]
public static class NotFoundProblemDetailsFactory
{
    public static ProblemDetails GetProblemDetails(string resource, string identifier) =>
        new()
        {
            Status = 404,
            Detail = $"Unable to find {resource} with Id: '{identifier}'.",
            Type = "https://www.Teams.api/errors/not-found",
            Title = "Resource Not Found",
            Extensions = new Dictionary<string, object?>
            {
                { "resource", resource },
                { "identifier", identifier }
            }
        };
}