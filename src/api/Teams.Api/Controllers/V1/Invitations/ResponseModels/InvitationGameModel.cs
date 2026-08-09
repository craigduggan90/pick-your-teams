using System.Diagnostics.CodeAnalysis;

namespace Teams.Api.Controllers.V1.Invitations.ResponseModels;

public record InvitationGameModel(string Id, DateTime StartTime, int Duration, string? Location)
{
    [ExcludeFromCodeCoverage]
    public static InvitationGameModel Example => new(
        Id: "c300336671ec45568ffbfc7235159132",
        StartTime: new DateTime(2026, 8, 10, 19, 00, 00, DateTimeKind.Utc),
        Duration: 60,
        Location: "Oak Leaf Leisure Centre");
}