using Swashbuckle.AspNetCore.Filters;
using System.Diagnostics.CodeAnalysis;
using Teams.Api.Controllers.V1.Games.ResponseModels;

namespace Teams.Api.Controllers.V1.Games.Examples;

[ExcludeFromCodeCoverage]
public class GameDetailModelExample : IExamplesProvider<GameDetailModel>
{
    public GameDetailModel GetExamples() => GameDetailModel.Example;
}