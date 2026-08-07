namespace Teams.Api.Controllers.V1.Games.RequestModels;

public record InvitePlayersRequestModel(IReadOnlyCollection<string> UserIdentifiers)
{
    public static InvitePlayersRequestModel Example => new(
        UserIdentifiers:
        [
            "joe@test.net",
            "user-tag",
            "jacqui@test.net",
            "_superplayer",
            "mike@other.com"
        ]);
}