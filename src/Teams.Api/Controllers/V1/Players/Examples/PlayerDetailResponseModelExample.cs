using Swashbuckle.AspNetCore.Filters;
using System.Diagnostics.CodeAnalysis;
using Teams.Api.Controllers.V1.Players.ResponseModels;

namespace Teams.Api.Controllers.V1.Players.Examples;

[ExcludeFromCodeCoverage]
public class PlayerDetailResponseModelExample : IExamplesProvider<PlayerDetailResponseModel>
{
    public PlayerDetailResponseModel GetExamples()
    {
        var exampleBase = PlayerResponseModelExample.GetExample();
        return new PlayerDetailResponseModel(
            exampleBase.Id,
            exampleBase.Name,
            exampleBase.Rating,
            new DateTime(2026, 07, 29, 15, 58, 32, DateTimeKind.Utc),
            new DateTime(2026, 07, 29, 16, 07, 15, DateTimeKind.Utc));
    }
}