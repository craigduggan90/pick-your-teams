using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Filters;
using System.Diagnostics.CodeAnalysis;
using Teams.Api.Infrastructure.Errors.Handlers;
using Teams.Core.Exceptions;

namespace Teams.Api.Infrastructure.Swagger.Examples.V1.Common;

[ExcludeFromCodeCoverage]
public class AccessDeniedProblemDetailsExample : IExamplesProvider<ProblemDetails>
{
    public ProblemDetails GetExamples() =>
        AccessDeniedExceptionHandler.GetProblemDetails(AccessDeniedException.ForOrganiserOrSelfOnly());
}