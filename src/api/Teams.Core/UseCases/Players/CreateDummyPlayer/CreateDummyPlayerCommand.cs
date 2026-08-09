using Teams.Core.CQRS;
using Teams.Domain.Entities;

namespace Teams.Core.UseCases.Players.CreateDummyPlayer;

public record CreateDummyPlayerCommand(string GameId, string DisplayName, int EstimatedRating) : IRequest<Player>;