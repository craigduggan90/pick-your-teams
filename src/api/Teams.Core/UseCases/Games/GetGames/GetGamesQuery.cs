using Teams.Core.CQRS;
using Teams.Domain.Entities;

namespace Teams.Core.UseCases.Games.GetGames;

public record GetGamesQuery(
    string? Location,
    DateTime? StartTimeFrom,
    DateTime? StartTimeTo,
    int? DurationFrom,
    int? DurationTo,
    int? TeamSize,
    DateTime? CreatedFrom,
    DateTime? CreatedTo,
    DateTime? ModifiedFrom,
    DateTime? ModifiedTo,
    int? PageSize,
    long? Cursor)
    : IRequest<IReadOnlyCollection<Game>>;