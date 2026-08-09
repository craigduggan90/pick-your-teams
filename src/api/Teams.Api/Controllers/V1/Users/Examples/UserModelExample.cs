using Swashbuckle.AspNetCore.Filters;
using System.Diagnostics.CodeAnalysis;
using Teams.Api.Controllers.V1.Users.ResponseModels;

namespace Teams.Api.Controllers.V1.Users.Examples;

[ExcludeFromCodeCoverage]
public class UserModelExample : IExamplesProvider<UserModel>
{
    public UserModel GetExamples() => UserModel.Example;
}