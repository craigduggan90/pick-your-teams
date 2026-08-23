namespace Teams.Authoriser.Auth;

/// <summary>The subset of Auth0's /userinfo response this authoriser needs.</summary>
public record UserInfo(string? Name, string? Email);