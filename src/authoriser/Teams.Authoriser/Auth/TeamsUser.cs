namespace Teams.Authoriser.Auth;

/// <summary>Mirrors Teams.Api's UserModel wire shape (camelCase response body).</summary>
public record TeamsUser(string Id, string Tag, string DisplayName, int Rating);