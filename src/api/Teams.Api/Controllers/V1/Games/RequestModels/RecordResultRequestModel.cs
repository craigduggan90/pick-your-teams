namespace Teams.Api.Controllers.V1.Games.RequestModels;

public record RecordResultRequestModel(string Winner)
{
    public static RecordResultRequestModel HomeTeamExample = new("Home");
    public static RecordResultRequestModel AwayTeamExample = new("Away");
    public static RecordResultRequestModel NoWinnerExample = new("None");
}