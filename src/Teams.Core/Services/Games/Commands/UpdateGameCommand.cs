namespace Teams.Core.Services.Games.Commands;

public record UpdateGameCommand(string Id, string? Location, DateTime StartTime, DateTime? EndTime);