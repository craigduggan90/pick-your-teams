using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Filters;

namespace Teams.Api.Infrastructure.Swagger.Examples.V1.Common;

public class ConcurrencyTokenMismatchExample : IExamplesProvider<ProblemDetails>
{
    public ProblemDetails GetExamples() =>
        new()
        {
            Title = "Precondition Failed",
            Type = "https://www.Teams.api/errors/concurrency",
            Status = 412,
            Detail = "Concurrency Token does not match current record state."
        };
}