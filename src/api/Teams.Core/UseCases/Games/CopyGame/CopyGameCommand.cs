using Teams.Core.CQRS;
using Teams.Domain.Entities;

namespace Teams.Core.UseCases.Games.CopyGame;

public record CopyGameCommand(string Id, DateTime StartTime) : IRequest<Game>;