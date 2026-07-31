using Teams.Core.CQRS;
using Teams.Domain.Entities;
using Teams.Domain.Enums;

namespace Teams.Core.UseCases.Players.GetPlayers;

public record GetPlayersQuery(
    string? GameId = null,
    string? DisplayName = null,
    string? UserId = null,
    int? RatingFrom = null,
    int? RatingTo = null,
    GameTeamEnum? Team = null,
    PlayerTypeEnum? Type = null,
    DateTime? CreatedFrom = null,
    DateTime? CreatedTo = null,
    DateTime? ModifiedFrom = null,
    DateTime? ModifiedTo = null,
    int? PageSize = null,
    long? Cursor = null) : IRequest<IReadOnlyCollection<Player>>;