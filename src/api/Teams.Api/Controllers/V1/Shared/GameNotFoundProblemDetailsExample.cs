using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Filters;
using System.Diagnostics.CodeAnalysis;
using Teams.Api.Infrastructure.Swagger.Examples.V1.Common;

namespace Teams.Api.Controllers.V1.Shared;

[ExcludeFromCodeCoverage]
public class GameNotFoundProblemDetailsExample : IExamplesProvider<ProblemDetails>
{
    public ProblemDetails GetExamples() =>
        NotFoundProblemDetailsFactory.GetProblemDetails("Game", "aab7f4f69f714a219c9c911374e2d5bf");
}