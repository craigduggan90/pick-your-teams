using Teams.Api.Controllers.V1.Jobs.ResponseModels;
using Teams.Domain.Enums;
using Swashbuckle.AspNetCore.Filters;

namespace Teams.Api.Infrastructure.Swagger.Examples.V1.Jobs;

public class JobResponseDetailModelExample : IExamplesProvider<JobResponseDetailModel>
{
    public JobResponseDetailModel GetExamples()
        => new(
            "3aff52756b1944489b1a3f85bf8d3d91",
            "76b06dd6-416c-4a86-a97c-7316158f1a95",
            "ea6e3216715d930fe856dab86ac5afbe",
            nameof(JobTypeEnum.ArchiveUserGroupJob),
            nameof(JobStatusEnum.Failed),
            new { Id = 1010 },
            new DateTime(2026, 7, 18, 13, 37, 14, DateTimeKind.Utc),
            new DateTime(2026, 7, 18, 14, 11, 25, DateTimeKind.Utc),
            new JobResponseErrorModel("JobTargetNotFound", "No UserGroup was found with identifier: 1010."));
}