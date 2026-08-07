using Swashbuckle.AspNetCore.Filters;
using System.Diagnostics.CodeAnalysis;
using Teams.Api.Controllers.V1.Games.RequestModels;

namespace Teams.Api.Controllers.V1.Games.Examples;

[ExcludeFromCodeCoverage]
public class RecordResultRequestModelExample : IMultipleExamplesProvider<RecordResultRequestModel>
{
    public IEnumerable<SwaggerExample<RecordResultRequestModel>> GetExamples()
    {
        yield return SwaggerExample.Create("Home Win", RecordResultRequestModel.HomeTeamExample);
        yield return SwaggerExample.Create("Away Win", RecordResultRequestModel.AwayTeamExample);
        yield return SwaggerExample.Create("Draw", RecordResultRequestModel.NoWinnerExample);
    }
}