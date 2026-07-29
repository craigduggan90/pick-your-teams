using Teams.Api.Controllers.V1.Jobs.ResponseModels;
using Swashbuckle.AspNetCore.Filters;

namespace Teams.Api.Infrastructure.Swagger.Examples.V1.Jobs;

public class JobResponseModelExample : IExamplesProvider<JobResponseModel>
{
    public JobResponseModel GetExamples() => JobResponseModelFactory.GetExample();
}