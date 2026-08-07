namespace Teams.Api.Controllers.V1.Users.ResponseModels;

public record UserModel(string Id, string Tag, string DisplayName, int Rating)
{
    public static UserModel Example => new(
        Id: "2d83bedc6fb7457283eedfa020cbb41f",
        Tag: "jane_smith",
        DisplayName: "Jane Smith",
        Rating: 1042);
}