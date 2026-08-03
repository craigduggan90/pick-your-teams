using Swashbuckle.AspNetCore.Filters;
using System.Diagnostics.CodeAnalysis;
using Teams.Api.Controllers.V1.Games.RequestModels;

namespace Teams.Api.Controllers.V1.Games.Examples;

[ExcludeFromCodeCoverage]
public class UpdateGameRequestModelExample : IExamplesProvider<UpdateGameRequestModel>
{
    public UpdateGameRequestModel GetExamples() => UpdateGameRequestModel.Example;
}