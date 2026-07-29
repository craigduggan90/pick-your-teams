using Teams.Api.Controllers.V1.Jobs.ResponseModels;
using Teams.Common.Pagination;
using Swashbuckle.AspNetCore.Filters;

namespace Teams.Api.Infrastructure.Swagger.Examples.V1.Jobs;

public class JobResponseModelPageExample : IExamplesProvider<PagedList<JobResponseModel>>
{
    public PagedList<JobResponseModel> GetExamples()
        => new([JobResponseModelFactory.GetExample()], "MTc4NDM3ODUzODQ5ODIxMw==", 1);
}