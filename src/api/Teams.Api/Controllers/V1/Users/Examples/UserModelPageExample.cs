using Swashbuckle.AspNetCore.Filters;
using System.Diagnostics.CodeAnalysis;
using Teams.Api.Controllers.V1.Users.ResponseModels;
using Teams.Common.Pagination;

namespace Teams.Api.Controllers.V1.Users.Examples;

[ExcludeFromCodeCoverage]
public class UserModelPageExample : IExamplesProvider<PagedList<UserModel>>
{
    public PagedList<UserModel> GetExamples() => new(
        Data: [UserModel.Example],
        Cursor: "MTc4NTUyNDI1ODQ0NQ==",
        Count: 1);
}