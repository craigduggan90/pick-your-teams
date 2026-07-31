namespace Teams.Api.Controllers.V1.Games.RequestModels;

public record UpdateGameRequestModel(string? Location, DateTime? StartTime, int? Duration)
{
    public static UpdateGameRequestModel Example => new(
        Location: "Longfield School",
        StartTime: new DateTime(2026, 07, 31, 19, 45, 00, DateTimeKind.Utc),
        Duration: 45);
}