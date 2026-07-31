using Teams.Core.CQRS;
using Teams.Domain.Entities;

namespace Teams.Core.UseCases.Players.GetPlayerById;

public record GetPlayerByIdQuery(string Id) : IRequest<Player>;