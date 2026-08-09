using Swashbuckle.AspNetCore.Filters;
using System.Diagnostics.CodeAnalysis;
using Teams.Api.Controllers.V1.Games.ResponseModels;
using Teams.Common.Pagination;

namespace Teams.Api.Controllers.V1.Games.Examples;

[ExcludeFromCodeCoverage]
public class GameModelPageExample : IExamplesProvider<PagedList<GameModel>>
{
    public PagedList<GameModel> GetExamples() => new(
        Data: [GameModel.Example],
        Cursor: "MTc4NTUyNDI1ODQ0NQ==",
        Count: 1);
}