using Swashbuckle.AspNetCore.Filters;
using Teams.Api.Controllers.V1.Players.ResponseModel;

namespace Teams.Api.Controllers.V1.Players.Examples;

public class PlayerModelExample : IMultipleExamplesProvider<PlayerModel>
{
    public IEnumerable<SwaggerExample<PlayerModel>> GetExamples() => [UserExample, DummyExample];

    private static readonly SwaggerExample<PlayerModel> UserExample = 
        SwaggerExample.Create("User", PlayerModel.UserExample);

    private static readonly SwaggerExample<PlayerModel> DummyExample = 
        SwaggerExample.Create("Dummy", PlayerModel.DummyExample);
}