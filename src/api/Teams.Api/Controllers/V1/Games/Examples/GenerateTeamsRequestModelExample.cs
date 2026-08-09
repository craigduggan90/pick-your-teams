using Swashbuckle.AspNetCore.Filters;
using System.Diagnostics.CodeAnalysis;
using Teams.Api.Controllers.V1.Games.RequestModels;

namespace Teams.Api.Controllers.V1.Games.Examples;

[ExcludeFromCodeCoverage]
public class GenerateTeamsRequestModelExample : IExamplesProvider<GenerateTeamsRequestModel>
{
    public GenerateTeamsRequestModel GetExamples() =>
        GenerateTeamsRequestModel.Example;
}