using Teams.Domain.Enums;

namespace Teams.Api.Controllers.V1.Games.ResponseModels;

public record GameDetailModel(
    string Id,
    string? Location,
    DateTime StartTime,
    int Duration,
    int TeamSize,
    string Status,
    string? Winner,
    int? HomeTeamRating,
    int? AwayTeamRating,
    DateTime Created,
    DateTime Modified)
{
    public static GameDetailModel Example => new(
        Id: "1e688dad723844f3b453b925157f05c4",
        Location: "Oak Leaf Leisure Centre",
        StartTime: new DateTime(2026, 07, 31, 20, 45, 00, DateTimeKind.Utc),
        Duration: 60,
        TeamSize: 5,
        Status: nameof(GameStatusEnum.Scheduled),
        Winner: nameof(GameTeamEnum.Away),
        HomeTeamRating: 5378,
        AwayTeamRating: 6417,
        Created: new DateTime(2026, 07, 30, 12, 14, 17, DateTimeKind.Utc),
        Modified: new DateTime(2026, 07, 31, 22, 18, 31, DateTimeKind.Utc));
}