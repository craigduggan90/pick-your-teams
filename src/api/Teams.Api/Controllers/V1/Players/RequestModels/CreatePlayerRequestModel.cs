using System.Diagnostics.CodeAnalysis;

namespace Teams.Api.Controllers.V1.Players.RequestModels;

public record CreatePlayerRequestModel(string GameId, string UserId)
{
    [ExcludeFromCodeCoverage]
    public static CreatePlayerRequestModel Example =>
        new("d31783ee1ebb4c71b0d2f029f461e469", "a070b4d8e5944a0fb4794929c815f9fa");
}