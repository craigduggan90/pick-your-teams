using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Filters;
using System.Diagnostics.CodeAnalysis;
using Teams.Api.Infrastructure.Swagger.Examples.V1.Common;

namespace Teams.Api.Controllers.V1.Shared;

[ExcludeFromCodeCoverage]
public class PlayerNotFoundProblemDetailsExample : IExamplesProvider<ProblemDetails>
{
    public ProblemDetails GetExamples() =>
        NotFoundProblemDetailsFactory.GetProblemDetails("Player", "6721fe515f1f4b52b1d2d51818c9fabf");
}