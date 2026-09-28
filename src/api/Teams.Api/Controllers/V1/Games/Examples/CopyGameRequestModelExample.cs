using Swashbuckle.AspNetCore.Filters;
using System.Diagnostics.CodeAnalysis;
using Teams.Api.Controllers.V1.Games.RequestModels;

namespace Teams.Api.Controllers.V1.Games.Examples;

[ExcludeFromCodeCoverage]
public class CopyGameRequestModelExample : IExamplesProvider<CopyGameRequestModel>
{
    public CopyGameRequestModel GetExamples() => CopyGameRequestModel.Example;
}