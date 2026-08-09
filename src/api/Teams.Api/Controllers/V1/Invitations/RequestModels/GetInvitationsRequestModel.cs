namespace Teams.Api.Controllers.V1.Invitations.RequestModels;

public record GetInvitationsRequestModel(
    string? GameId = null,
    string? UserId = null,
    string? EmailAddress = null,
    string? Status = null,
    DateTime? CreatedFrom = null,
    DateTime? CreatedTo = null,
    DateTime? ModifiedFrom = null,
    DateTime? ModifiedTo = null,
    int? PageSize = null,
    string? Cursor = null);