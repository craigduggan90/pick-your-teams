using System.Diagnostics.CodeAnalysis;
using Teams.Domain.Enums;

namespace Teams.Api.Controllers.V1.Players.ResponseModel;

public record PlayerModel(
    string Id,
    string GameId,
    string? UserId,
    string? Tag,
    string Type,
    string? DisplayName,
    int Rating,
    string Team)
{
    [ExcludeFromCodeCoverage]
    public static PlayerModel UserExample => new(
        "2f735c4a01f14fcc8b310117f58730ef",
        "c6a893da2b2d4b6b83f82a8e3573f861",
        "f41aa2a3d64748f7828789de45999923",
        "marcusaurelius",
        nameof(PlayerTypeEnum.User),
        "Marcus Aurelius",
        161,
        nameof(GameTeamEnum.None));

    [ExcludeFromCodeCoverage]
    public static PlayerModel DummyExample => new(
        "3c883ef7ae9e446d828530a1490fe580",
        "c6a893da2b2d4b6b83f82a8e3573f861",
        null,
        null,
        nameof(PlayerTypeEnum.Dummy),
        "Didius Julianus",
        800,
        nameof(GameTeamEnum.None));
}