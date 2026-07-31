namespace Teams.Api.Controllers.V1.Games.RequestModels;

public record CreateGameRequestModel(string? Location, DateTime StartTime, int Duration, int TeamSize)
{
    public static CreateGameRequestModel Example => new(
        Location: "Oak Leaf Leisure Centre",
        StartTime: new DateTime(2026, 07, 31, 20, 45, 00, DateTimeKind.Utc),
        Duration: 60,
        TeamSize: 5);
}