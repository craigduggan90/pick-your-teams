using Swashbuckle.AspNetCore.Filters;
using Teams.Api.Controllers.V1.Players.RequestModels;

namespace Teams.Api.Controllers.V1.Players.Examples;

public class CreateDummyPlayerRequestModelExample : IExamplesProvider<CreateDummyPlayerRequestModel>
{
    public CreateDummyPlayerRequestModel GetExamples() => CreateDummyPlayerRequestModel.Example;
}