using Swashbuckle.AspNetCore.Filters;
using Teams.Api.Controllers.V1.Games.RequestModels;

namespace Teams.Api.Controllers.V1.Games.Examples;

public class SetTeamsRequestModelExample : IExamplesProvider<SetTeamsRequestModel>
{
    public SetTeamsRequestModel GetExamples() => SetTeamsRequestModel.Example;
}