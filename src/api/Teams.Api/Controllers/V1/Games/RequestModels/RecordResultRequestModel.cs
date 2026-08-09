using System.Diagnostics.CodeAnalysis;

namespace Teams.Api.Controllers.V1.Games.RequestModels;

public record RecordResultRequestModel(string Winner)
{
    [ExcludeFromCodeCoverage]
    public static RecordResultRequestModel HomeTeamExample => new("Home");

    [ExcludeFromCodeCoverage]
    public static RecordResultRequestModel AwayTeamExample => new("Away");

    [ExcludeFromCodeCoverage]
    public static RecordResultRequestModel NoWinnerExample => new("None");
}