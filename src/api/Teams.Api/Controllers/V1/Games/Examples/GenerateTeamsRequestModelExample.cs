using Swashbuckle.AspNetCore.Filters;
using Teams.Api.Controllers.V1.Games.RequestModels;

namespace Teams.Api.Controllers.V1.Games.Examples;

public class GenerateTeamsRequestModelExample : IExamplesProvider<GenerateTeamsRequestModel>
{
    public GenerateTeamsRequestModel GetExamples() =>
        GenerateTeamsRequestModel.Example;
}