using Teams.Core.CQRS;
using Teams.Domain.Entities;

namespace Teams.Core.UseCases.Games.CreateGame;

public record CreateGameCommand(string? Location, DateTime StartTime, int Duration, int TeamSize)
    : IRequest<Game>;