using Teams.Core.CQRS;
using Teams.Domain.Entities;

namespace Teams.Core.UseCases.Players.CreatePlayer;

public record CreatePlayerCommand(string GameId, string UserId) : IRequest<Player>;