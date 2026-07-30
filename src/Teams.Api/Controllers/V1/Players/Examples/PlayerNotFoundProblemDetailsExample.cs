using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Filters;
using System.Diagnostics.CodeAnalysis;
using Teams.Api.Infrastructure.Swagger.Examples.V1.Common;
using Teams.Domain.Entities;

namespace Teams.Api.Controllers.V1.Players.Examples;

[ExcludeFromCodeCoverage]
public class PlayerNotFoundProblemDetailsExample : IExamplesProvider<ProblemDetails>
{
    public ProblemDetails GetExamples()
        => NotFoundProblemDetailsFactory.GetProblemDetails(nameof(Player), "5955308aa7074a3eb89840484d286b8d");
}