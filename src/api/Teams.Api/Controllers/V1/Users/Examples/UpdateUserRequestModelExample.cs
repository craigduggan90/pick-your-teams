using Swashbuckle.AspNetCore.Filters;
using System.Diagnostics.CodeAnalysis;
using Teams.Api.Controllers.V1.Users.RequestModels;

namespace Teams.Api.Controllers.V1.Users.Examples;

[ExcludeFromCodeCoverage]
public class UpdateUserRequestModelExample : IExamplesProvider<UpdateUserRequestModel>
{
    public UpdateUserRequestModel GetExamples() => UpdateUserRequestModel.Example;
}