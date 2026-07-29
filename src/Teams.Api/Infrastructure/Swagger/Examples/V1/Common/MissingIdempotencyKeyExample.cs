using Microsoft.AspNetCore.Mvc;
using Teams.Common;
using Swashbuckle.AspNetCore.Filters;

namespace Teams.Api.Infrastructure.Swagger.Examples.V1.Common;

public class MissingIdempotencyKeyExample : IExamplesProvider<ProblemDetails>
{
    public ProblemDetails GetExamples()
        => MissingHeaderProblemDetailsFactory.GetProblemDetails(Constants.IdempotencyHeaderKey);
}