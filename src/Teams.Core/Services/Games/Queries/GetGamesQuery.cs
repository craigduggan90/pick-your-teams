namespace Teams.Core.Services.Games.Queries;

public record GetGamesQuery(
    string? Location = null,
    DateTime? StartTimeFrom = null,
    DateTime? StartTimeTo = null,
    DateTime? EndTimeFrom = null,
    DateTime? EndTimeTo = null,
    int? TeamSize = null,
    DateTime? CreatedFrom = null,
    DateTime? CreatedTo = null,
    DateTime? ModifiedFrom = null,
    DateTime? ModifiedTo = null,
    long? Cursor = null,
    int? PageSize = null);