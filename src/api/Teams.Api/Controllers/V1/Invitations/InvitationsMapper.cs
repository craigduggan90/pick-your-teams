using Teams.Api.Controllers.V1.Invitations.RequestModels;
using Teams.Api.Controllers.V1.Invitations.ResponseModels;
using Teams.Common.Pagination;
using Teams.Core.UseCases.Invitations.CreateInvitations;
using Teams.Core.UseCases.Invitations.GetInvitations;
using Teams.Domain.Entities;
using Teams.Domain.Enums;

namespace Teams.Api.Controllers.V1.Invitations;

public static class InvitationsMapper
{
    public static InvitationModel ToModel(this Invitation invitation) => new(
        Id: invitation.Id,
        Status: invitation.Status.ToString(),
        Game: invitation.Game.ToModel(),
        Organiser: invitation.Game.Organiser.ToModel());

    public static InvitationGameModel ToModel(this Game game) => new(
        Id: game.Id,
        StartTime: game.StartTime,
        Duration: game.Duration,
        Location: game.Location);

    public static InvitationOrganiserModel ToModel(this User user) => new(
        Id: user.Id,
        Tag: user.Tag,
        DisplayName: user.DisplayName);

    public static InvitationDetailModel ToDetailModel(this Invitation invitation) => new(
        Id: invitation.Id,
        Status: invitation.Status.ToString(),
        Game: invitation.Game.ToModel(),
        Organiser: invitation.Game.Organiser.ToModel(),
        Created: invitation.DateCreated,
        Modified: invitation.DateModified);

    public static CreateInvitationsCommand ToCommand(this CreateInvitationsRequestModel model) => new(
        GameId: model.GameId,
        UserTags: model.UserTags);

    public static GetInvitationsQuery ToQuery(this GetInvitationsRequestModel model) => new(
        GameId: model.GameId,
        UserId: model.UserId,
        EmailAddress: model.EmailAddress,
        Status: Enum.TryParse<InvitationStatusEnum>(model.Status, true, out var status) ? status : null,
        CreatedFrom: model.CreatedFrom,
        CreatedTo: model.CreatedTo,
        ModifiedFrom: model.ModifiedFrom,
        ModifiedTo: model.ModifiedTo,
        PageSize: model.PageSize,
        Cursor: model.Cursor.TryDecodeCursor(out var c) ? c : null);
}