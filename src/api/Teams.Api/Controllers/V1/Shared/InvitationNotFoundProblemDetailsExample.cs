using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Filters;
using System.Diagnostics.CodeAnalysis;
using Teams.Api.Infrastructure.Swagger.Examples.V1.Common;

namespace Teams.Api.Controllers.V1.Shared;

[ExcludeFromCodeCoverage]
public class InvitationNotFoundProblemDetailsExample : IExamplesProvider<ProblemDetails>
{
    public ProblemDetails GetExamples() =>
        NotFoundProblemDetailsFactory.GetProblemDetails("Invitation", "4a0b023b37c54affbbeb9fac492902ca");
}