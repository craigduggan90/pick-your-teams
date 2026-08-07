using System.Diagnostics.CodeAnalysis;
using Teams.Domain.Enums;

namespace Teams.Api.Controllers.V1.Games.ResponseModels;

public record GameModel(string Id, string? Location, DateTime StartTime, int Duration, int TeamSize, string Status)
{
    [ExcludeFromCodeCoverage]
    public static GameModel Example => new(
        Id: "1e688dad723844f3b453b925157f05c4",
        Location: "Oak Leaf Leisure Centre",
        StartTime: new DateTime(2026, 07, 31, 20, 45, 00, DateTimeKind.Utc),
        Duration: 60,
        TeamSize: 5,
        Status: nameof(GameStatusEnum.Scheduled));
}