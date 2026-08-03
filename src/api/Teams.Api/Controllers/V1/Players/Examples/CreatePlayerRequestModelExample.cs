using Swashbuckle.AspNetCore.Filters;
using Teams.Api.Controllers.V1.Players.RequestModels;

namespace Teams.Api.Controllers.V1.Players.Examples;

public class CreatePlayerRequestModelExample : IExamplesProvider<CreatePlayerRequestModel>
{
    public CreatePlayerRequestModel GetExamples() => CreatePlayerRequestModel.Example;
}