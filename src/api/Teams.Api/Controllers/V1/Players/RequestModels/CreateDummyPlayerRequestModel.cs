using System.Diagnostics.CodeAnalysis;

namespace Teams.Api.Controllers.V1.Players.RequestModels;

public record CreateDummyPlayerRequestModel(string GameId, string DisplayName, int EstimatedRating)
{
    [ExcludeFromCodeCoverage]
    public static CreateDummyPlayerRequestModel Example =>
        new("d31783ee1ebb4c71b0d2f029f461e469", "Jess B", 1371);
}