namespace Teams.Api.Controllers.V1.Players.RequestModels;

public record GetPlayersRequestModel(
    string? Name = null,
    int? RatingFrom = null,
    int? RatingTo = null,
    DateTime? CreatedFrom = null,
    DateTime? CreatedTo = null,
    DateTime? ModifiedFrom = null,
    DateTime? ModifiedTo = null,
    string? Cursor = null,
    int? PageSize = null);