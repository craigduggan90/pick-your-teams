using Swashbuckle.AspNetCore.Filters;
using System.Diagnostics.CodeAnalysis;
using Teams.Api.Controllers.V1.Players.RequestModels;

namespace Teams.Api.Controllers.V1.Players.Examples;

[ExcludeFromCodeCoverage]
public class UpdatePlayerRequestModelExample : IExamplesProvider<UpdatePlayerRequestModel>
{
    public UpdatePlayerRequestModel GetExamples()
        => new("Joe Bloggs");
}