using Swashbuckle.AspNetCore.Filters;
using Teams.Api.Controllers.V1.Games.RequestModels;

namespace Teams.Api.Controllers.V1.Games.Examples;

public class InvitePlayersRequestModelExample : IExamplesProvider<InvitePlayersRequestModel>
{
    public InvitePlayersRequestModel GetExamples() => InvitePlayersRequestModel.Example;
}