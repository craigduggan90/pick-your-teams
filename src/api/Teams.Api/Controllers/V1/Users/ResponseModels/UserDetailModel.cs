using System.Diagnostics.CodeAnalysis;

namespace Teams.Api.Controllers.V1.Users.ResponseModels;

public record UserDetailModel(
    string Id,
    string Tag,
    string DisplayName,
    int Rating,
    string Email,
    string? Mobile,
    DateTime Created,
    DateTime Modified)
{
    [ExcludeFromCodeCoverage]
    public static UserDetailModel Example => new(
        Id: "2d83bedc6fb7457283eedfa020cbb41f",
        Tag: "jane_smith",
        DisplayName: "Jane Smith",
        Rating: 1042,
        Email: "jane.smith@example.com",
        Mobile: "+447700900123",
        Created: new DateTime(2026, 07, 30, 12, 14, 17, DateTimeKind.Utc),
        Modified: new DateTime(2026, 07, 31, 22, 18, 31, DateTimeKind.Utc));
}