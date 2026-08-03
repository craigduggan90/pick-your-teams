using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Filters;
using Teams.Common;

namespace Teams.Api.Infrastructure.Swagger.Examples.V1.Common;

public class MissingIdempotencyKeyExample : IExamplesProvider<ProblemDetails>
{
    public ProblemDetails GetExamples()
        => MissingHeaderProblemDetailsFactory.GetProblemDetails(Constants.IdempotencyHeaderKey);
}