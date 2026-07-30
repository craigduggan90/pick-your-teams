using Swashbuckle.AspNetCore.Filters;
using System.Diagnostics.CodeAnalysis;
using Teams.Api.Controllers.V1.Players.ResponseModels;

namespace Teams.Api.Controllers.V1.Players.Examples;

[ExcludeFromCodeCoverage]
public class PlayerResponseModelExample : IExamplesProvider<PlayerResponseModel>
{
    public PlayerResponseModel GetExamples() => GetExample();

    internal static PlayerResponseModel GetExample()
        => new("5955308aa7074a3eb89840484d286b8d", "Joe Bloggs", 1400);
}