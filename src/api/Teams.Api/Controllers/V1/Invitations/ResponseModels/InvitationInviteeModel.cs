using System.Diagnostics.CodeAnalysis;

namespace Teams.Api.Controllers.V1.Invitations.ResponseModels;

public record InvitationInviteeModel(string Id, string Tag, string DisplayName)
{
    [ExcludeFromCodeCoverage]
    public static InvitationInviteeModel Example => new(
        Id: "3f7a2c91b8d44e6fa1c2d3e4f5a6b7c8",
        Tag: "monkey-duster",
        DisplayName: "Jordan Monk");
}