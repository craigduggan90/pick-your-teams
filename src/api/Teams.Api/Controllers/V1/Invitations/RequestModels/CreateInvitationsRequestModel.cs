namespace Teams.Api.Controllers.V1.Invitations.RequestModels;

public record CreateInvitationsRequestModel(string GameId, IReadOnlyCollection<string> UserTags)
{
    public static CreateInvitationsRequestModel Example => new(
        GameId: "2fc666cb6d1546058a0c72d7492b4830",
        UserTags:
        [
            "monkey-duster",
            "_emc2",
            "tha.carter.six",
            "f30",
            "owweeennnn"
        ]);
};