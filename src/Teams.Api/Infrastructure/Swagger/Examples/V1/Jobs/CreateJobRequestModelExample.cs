using Teams.Api.Controllers.V1.Jobs.RequestModels;
using Teams.Domain.Enums;
using Swashbuckle.AspNetCore.Filters;
using System.Text.Json;

namespace Teams.Api.Infrastructure.Swagger.Examples.V1.Jobs;

public class CreateJobRequestModelExample : IExamplesProvider<CreateJobRequestModel>
{
    public CreateJobRequestModel GetExamples() =>
        new CreateJobRequestModel(
            nameof(JobTypeEnum.ArchiveProjectJob),
            JsonSerializer.SerializeToElement(new
            {
                Property = "value",
                Nested = new { OtherProperty = "otherValue" }
            }));
}