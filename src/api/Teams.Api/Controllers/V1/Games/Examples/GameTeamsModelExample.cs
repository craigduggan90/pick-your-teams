using Swashbuckle.AspNetCore.Filters;
using Teams.Api.Controllers.V1.Games.ResponseModels;

namespace Teams.Api.Controllers.V1.Games.Examples;

public class GameTeamsModelExample : IMultipleExamplesProvider<GameTeamsModel>
{
    private const string SuccessExample = "Success";
    private const string EmptyExample = "No Combinations Meeting Differential";

    public IEnumerable<SwaggerExample<GameTeamsModel>> GetExamples()
    {
        yield return SwaggerExample.Create(SuccessExample, GameTeamsModel.Example);
        yield return SwaggerExample.Create(EmptyExample, GameTeamsModel.Empty);
    }
}