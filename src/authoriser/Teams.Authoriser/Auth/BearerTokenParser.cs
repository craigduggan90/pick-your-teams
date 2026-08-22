namespace Teams.Authoriser.Auth;

/// <summary>Extracts the raw token from a `Bearer &lt;token&gt;` Authorization header value.</summary>
public static class BearerTokenParser
{
    private const string BearerPrefix = "Bearer ";

    public static bool TryGetBearerToken(string? authorizationHeader, out string token)
    {
        token = string.Empty;

        if (string.IsNullOrWhiteSpace(authorizationHeader)
            || !authorizationHeader.StartsWith(BearerPrefix, StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        token = authorizationHeader[BearerPrefix.Length..].Trim();
        return token.Length > 0;
    }
}
