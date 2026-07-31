using Swashbuckle.AspNetCore.Filters;
using Teams.Api.Controllers.V1.Players.ResponseModel;

namespace Teams.Api.Controllers.V1.Players.Examples;

public class PlayerDetailModelExample : IMultipleExamplesProvider<PlayerDetailModel>
{
    public IEnumerable<SwaggerExample<PlayerDetailModel>> GetExamples() => [UserExample, DummyExample];

    private static readonly SwaggerExample<PlayerDetailModel> UserExample = 
        SwaggerExample.Create("User", PlayerDetailModel.UserExample);

    private static readonly SwaggerExample<PlayerDetailModel> DummyExample = 
        SwaggerExample.Create("Dummy", PlayerDetailModel.DummyExample);
}