using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Filters;
using System.Diagnostics.CodeAnalysis;
using Teams.Api.Exceptions;
using Teams.Api.Infrastructure.Errors.Handlers;

namespace Teams.Api.Infrastructure.Swagger.Examples.V1.Common;

[ExcludeFromCodeCoverage]
public class MissingScopeProblemDetailsExample : IExamplesProvider<ProblemDetails>
{
    public ProblemDetails GetExamples() =>
        MissingScopeExceptionHandler.GetProblemDetails(new MissingScopeException("authoriser"));
}