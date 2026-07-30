namespace Teams.Core.Services.Players.Queries;

public record GetPlayersQuery(
    string? Name = null,
    int? RatingFrom = null,
    int? RatingTo = null,
    DateTime? CreatedFrom = null,
    DateTime? CreatedTo = null,
    DateTime? ModifiedFrom = null,
    DateTime? ModifiedTo = null,
    long? Cursor = null,
    int? PageSize = null);