using Swashbuckle.AspNetCore.Filters;
using System.Diagnostics.CodeAnalysis;
using Teams.Api.Controllers.V1.Players.ResponseModels;
using Teams.Common.Pagination;

namespace Teams.Api.Controllers.V1.Players.Examples;

[ExcludeFromCodeCoverage]
public class PlayerResponseModelPageExample : IExamplesProvider<PagedList<PlayerResponseModel>>
{
    public PagedList<PlayerResponseModel> GetExamples()
        => new PagedList<PlayerResponseModel>(
            [PlayerResponseModelExample.GetExample()],
            CursorConverter.TryEncodeCursor(1785336327840, out var cursor) ? cursor : "no",
            1);
}