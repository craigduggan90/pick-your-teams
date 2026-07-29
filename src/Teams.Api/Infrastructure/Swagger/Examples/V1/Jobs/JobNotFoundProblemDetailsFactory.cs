using Microsoft.AspNetCore.Mvc;
using Teams.Api.Infrastructure.Swagger.Examples.V1.Common;
using Teams.Domain.Entities;
using Swashbuckle.AspNetCore.Filters;

namespace Teams.Api.Infrastructure.Swagger.Examples.V1.Jobs;

public class JobNotFoundProblemDetailsFactory : IExamplesProvider<ProblemDetails>
{
    public ProblemDetails GetExamples() =>
        NotFoundProblemDetailsFactory.GetProblemDetails(nameof(Job), "9ab662b7adc545198696085b8c61ec93");

}