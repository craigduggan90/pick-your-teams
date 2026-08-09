using Swashbuckle.AspNetCore.Filters;
using System.Diagnostics.CodeAnalysis;
using Teams.Api.Controllers.V1.Players.RequestModels;

namespace Teams.Api.Controllers.V1.Players.Examples;

[ExcludeFromCodeCoverage]
public class CreateDummyPlayerRequestModelExample : IExamplesProvider<CreateDummyPlayerRequestModel>
{
    public CreateDummyPlayerRequestModel GetExamples() => CreateDummyPlayerRequestModel.Example;
}