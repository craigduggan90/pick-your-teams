using System.Diagnostics.CodeAnalysis;

namespace Teams.Api.Controllers.V1.Games.RequestModels;

public record CopyGameRequestModel(DateTime StartTime)
{
    [ExcludeFromCodeCoverage]
    public static CopyGameRequestModel Example => new(
        StartTime: new DateTime(2026, 08, 07, 20, 45, 00, DateTimeKind.Utc));
}