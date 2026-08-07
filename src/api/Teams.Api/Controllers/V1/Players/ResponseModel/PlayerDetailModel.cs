using System.Diagnostics.CodeAnalysis;
using Teams.Domain.Enums;

namespace Teams.Api.Controllers.V1.Players.ResponseModel;

public record PlayerDetailModel(
    string Id,
    string GameId,
    string? UserId,
    string Type,
    string DisplayName,
    int Rating,
    int? RatingChange,
    string Team,
    DateTime Created,
    DateTime Modified)
{
    [ExcludeFromCodeCoverage]
    public static PlayerDetailModel UserExample => new(
        "2f735c4a01f14fcc8b310117f58730ef",
        "c6a893da2b2d4b6b83f82a8e3573f861",
        "f41aa2a3d64748f7828789de45999923",
        nameof(PlayerTypeEnum.User),
        "Marcus Aurelius",
        161,
        null,
        nameof(GameTeamEnum.None),
        new DateTime(2026, 8, 1, 14, 05, 12, DateTimeKind.Utc),
        new DateTime(2026, 8, 1, 14, 05, 12, DateTimeKind.Utc));

    [ExcludeFromCodeCoverage]
    public static PlayerDetailModel DummyExample => new(
        "3c883ef7ae9e446d828530a1490fe580",
        "c6a893da2b2d4b6b83f82a8e3573f861",
        null,
        nameof(PlayerTypeEnum.Dummy),
        "Didius Julianus",
        800,
        null,
        nameof(GameTeamEnum.None),
        new DateTime(2026, 8, 1, 14, 06, 14, DateTimeKind.Utc),
        new DateTime(2026, 8, 2, 08, 08, 05, DateTimeKind.Utc));
}