namespace Teams.Api.Controllers.V1.Games.RequestModels;

public record GetGamesRequestModel(
    string? Location = null,
    DateTime? StartTimeFrom = null,
    DateTime? StartTimeTo = null,
    int? DurationFrom = null,
    int? DurationTo = null,
    int? TeamSize = null,
    string? Status = null,
    DateTime? CreatedFrom = null,
    DateTime? CreatedTo = null,
    DateTime? ModifiedFrom = null,
    DateTime? ModifiedTo = null,
    int? PageSize = null,
    string? Cursor = null);