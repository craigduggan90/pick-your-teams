using System.Diagnostics.CodeAnalysis;

namespace Teams.Api.Controllers.V1.Games.RequestModels;

public record SetTeamsRequestModel(
    IReadOnlyCollection<string> HomeTeamIds,
    IReadOnlyCollection<string> AwayTeamIds)
{
    [ExcludeFromCodeCoverage]
    public static SetTeamsRequestModel Example => new(
        HomeTeamIds:
        [
            "8bb2f1594d544690812e144d179be647",
            "66fc7b7323324e90a414c0eea30cf4f7",
            "852ede9f943f4444bd20520df336ead8",
            "9cd9d549775e4177a0f4fca1d62fe551",
            "0ec1d9ee024641caa35ca14ce8714095"
        ],
        AwayTeamIds:
        [
            "5d1292e79345430ab231dcb3737c3e2d",
            "9c1ace5f46a94d83b24f9e3524fac1ff",
            "c249a477b2334f7b9231b6d983565a61",
            "7ca2115eebac4bbb8b137f42e9744125",
            "ec86a509bea447bdb3948f7f8e4fce61"
        ]);
}