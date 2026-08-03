using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Filters;
using Teams.Common;

namespace Teams.Api.Infrastructure.Swagger.Examples.V1.Common;

public class MissingConcurrencyTokenExample : IExamplesProvider<ProblemDetails>
{
    public ProblemDetails GetExamples()
        => MissingHeaderProblemDetailsFactory.GetProblemDetails(Constants.IfMatchHeaderKey);
}