using Teams.Core.CQRS;
using Teams.Domain.Entities;

namespace Teams.Core.UseCases.Games.UpdateGame;

public record UpdateGameCommand(string Id, string? Location, DateTime? StartTime, int? Duration) : IRequest<Game>;