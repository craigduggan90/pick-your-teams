using System.Diagnostics.CodeAnalysis;
using Teams.Domain.Enums;

namespace Teams.Api.Controllers.V1.Invitations.ResponseModels;

public record InvitationModel(
    string Id,
    string Status,
    InvitationGameModel Game,
    InvitationOrganiserModel? Organiser,
    InvitationInviteeModel? Invitee,
    DateTime Created)
{
    [ExcludeFromCodeCoverage]
    public static InvitationModel Example => new(
        Id: "67c300442e0241329c362d7f8d2af856",
        Status: nameof(InvitationStatusEnum.Accepted),
        Game: InvitationGameModel.Example,
        Organiser: InvitationOrganiserModel.Example,
        Invitee: InvitationInviteeModel.Example,
        Created: new DateTime(2026, 07, 27, 9, 31, 46, DateTimeKind.Utc));
}