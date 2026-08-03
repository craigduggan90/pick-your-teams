namespace Teams.Api.Controllers.V1.Players.RequestModels;

public record GetPlayersRequestModel(
    string? GameId = null,
    string? DisplayName = null,
    string? UserId = null,
    int? RatingFrom = null,
    int? RatingTo = null,
    string? Team = null,
    string? Type = null,
    DateTime? CreatedFrom = null,
    DateTime? CreatedTo = null,
    DateTime? ModifiedFrom = null,
    DateTime? ModifiedTo = null,
    int? PageSize = null,
    string? Cursor = null);