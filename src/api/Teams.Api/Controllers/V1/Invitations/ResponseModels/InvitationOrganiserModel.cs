namespace Teams.Api.Controllers.V1.Invitations.ResponseModels;

public record InvitationOrganiserModel(string Id, string Tag, string DisplayName)
{
    public static InvitationOrganiserModel Example => new(
        Id: "a694bc382d854d8385e79b2fce684090",
        Tag: "little-bobby-tables",
        DisplayName: "Robert D. Tables");
}