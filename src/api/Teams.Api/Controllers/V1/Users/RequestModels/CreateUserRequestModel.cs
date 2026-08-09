using System.Diagnostics.CodeAnalysis;

namespace Teams.Api.Controllers.V1.Users.RequestModels;

public record CreateUserRequestModel(string DisplayName, string ExternalId, string Email, string? Mobile)
{
    [ExcludeFromCodeCoverage]
    public static CreateUserRequestModel Example => new(
        DisplayName: "Jane Smith",
        ExternalId: "auth0|64f1a2b3c4d5e6f7a8b9c0d1",
        Email: "jane.smith@example.com",
        Mobile: "+447700900123");
}