using Teams.Core.CQRS;
using Teams.Domain.Entities;

namespace Teams.Core.UseCases.Players.DeletePlayer;

public record DeletePlayerCommand(string Id) : IRequest<Player>;