using Swashbuckle.AspNetCore.Filters;
using System.Diagnostics.CodeAnalysis;
using Teams.Api.Controllers.V1.Users.ResponseModels;

namespace Teams.Api.Controllers.V1.Users.Examples;

[ExcludeFromCodeCoverage]
public class UserDetailModelExample : IExamplesProvider<UserDetailModel>
{
    public UserDetailModel GetExamples() => UserDetailModel.Example;
}