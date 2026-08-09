using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Filters;
using System.Diagnostics.CodeAnalysis;
using Teams.Api.Infrastructure.Swagger.Examples.V1.Common;

namespace Teams.Api.Controllers.V1.Shared;

[ExcludeFromCodeCoverage]
public class UserNotFoundProblemDetailsExample : IExamplesProvider<ProblemDetails>
{
    public ProblemDetails GetExamples() =>
        NotFoundProblemDetailsFactory.GetProblemDetails("User", "2d83bedc6fb7457283eedfa020cbb41f");
}