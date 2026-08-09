using Teams.Core.CQRS;
using Teams.Domain.Entities;
using Teams.Domain.Enums;

namespace Teams.Core.UseCases.Games.GetGames;

public record GetGamesQuery(
    string? Location,
    DateTime? StartTimeFrom,
    DateTime? StartTimeTo,
    int? DurationFrom,
    int? DurationTo,
    int? TeamSize,
    GameStatusEnum? Status,
    DateTime? CreatedFrom,
    DateTime? CreatedTo,
    DateTime? ModifiedFrom,
    DateTime? ModifiedTo,
    int? PageSize,
    long? Cursor)
    : IRequest<IReadOnlyCollection<Game>>;