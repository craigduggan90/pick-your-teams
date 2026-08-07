namespace Teams.Api.Controllers.V1.Users.RequestModels;

public record UpdateUserRequestModel(string? Tag, string? DisplayName, string? Email, string? Mobile)
{
    public static UpdateUserRequestModel Example => new(
        Tag: "jane_smith",
        DisplayName: "Jane Smith",
        Email: "jane.smith@example.com",
        Mobile: "+447700900123");
}