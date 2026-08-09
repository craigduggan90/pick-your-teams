using Swashbuckle.AspNetCore.Filters;
using System.Diagnostics.CodeAnalysis;
using Teams.Api.Controllers.V1.Players.ResponseModel;
using Teams.Common.Pagination;

namespace Teams.Api.Controllers.V1.Players.Examples;

[ExcludeFromCodeCoverage]
public class PlayerModelPageExample : IExamplesProvider<PagedList<PlayerModel>>
{
    public PagedList<PlayerModel> GetExamples() => new PagedList<PlayerModel>(
        [PlayerModel.UserExample, PlayerModel.DummyExample],
        "MTc4NTU4OTk4MTkyNA==",
        2);
}