using Microsoft.AspNetCore.Mvc;
using System.Diagnostics.CodeAnalysis;

namespace Teams.Api.Infrastructure.Swagger.Examples.V1.Common;

[ExcludeFromCodeCoverage]
public static class MissingHeaderProblemDetailsFactory
{
    public static ProblemDetails GetProblemDetails(string headerName) =>
        new()
        {
            Title = "Precondition Required",
            Status = 428,
            Type = "https://www.Teams.api/errors/missing-header",
            Detail = $"'{headerName}' header value is required."
        };
}