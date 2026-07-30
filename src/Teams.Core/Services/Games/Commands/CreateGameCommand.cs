namespace Teams.Core.Services.Games.Commands;

public record CreateGameCommand(string? Location, DateTime StartTime, DateTime? EndTime, int TeamSize);