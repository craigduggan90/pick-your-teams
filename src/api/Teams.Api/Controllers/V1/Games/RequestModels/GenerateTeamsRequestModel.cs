namespace Teams.Api.Controllers.V1.Games.RequestModels;

public record GenerateTeamsRequestModel(
    IEnumerable<string> HomeTeamSeedIds,
    IEnumerable<string> AwayTeamSeedIds,
    int Differential)
{
    public static GenerateTeamsRequestModel Example => new(
        HomeTeamSeedIds:
        [
            "d343f0b9f7af47f8b1b8f351e53429dc",
            "7bbc8e6fc60c451b95828d8f0c1c90dd",
            "27c3f11325d0431dac847ed04b639566"
        ],
        AwayTeamSeedIds:
        [
            "c8e6d6292715405082ba3723996cb058",
            "dcc7922a5fc945fa969eb10f7c625bb2"
        ],
        Differential: 200);
}