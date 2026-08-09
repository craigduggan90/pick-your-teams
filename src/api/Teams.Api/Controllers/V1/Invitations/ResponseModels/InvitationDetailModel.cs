using System.Diagnostics.CodeAnalysis;
using Teams.Domain.Enums;

namespace Teams.Api.Controllers.V1.Invitations.ResponseModels;

public record InvitationDetailModel(
    string Id,
    string Status,
    InvitationGameModel Game,
    InvitationOrganiserModel Organiser,
    DateTime Created,
    DateTime Modified)
{
    [ExcludeFromCodeCoverage]
    public static InvitationDetailModel Example => new(
        Id: "67c300442e0241329c362d7f8d2af856",
        Status: nameof(InvitationStatusEnum.Declined),
        Game: InvitationGameModel.Example,
        Organiser: InvitationOrganiserModel.Example,
        Created: new DateTime(2026, 07, 27, 9, 31, 46, DateTimeKind.Utc),
        Modified: new DateTime(2026, 07, 31, 17, 42, 17, DateTimeKind.Utc));
}