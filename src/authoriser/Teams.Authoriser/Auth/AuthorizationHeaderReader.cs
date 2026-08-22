namespace Teams.Authoriser.Auth;

/// <summary>Pulls the `Authorization` header value off an incoming authorizer request, case-insensitively.</summary>
public static class AuthorizationHeaderReader
{
    public static string? GetAuthorizationHeader(IDictionary<string, string>? headers)
    {
        if (headers is null)
        {
            return null;
        }

        foreach (var header in headers)
        {
            if (string.Equals(header.Key, "Authorization", StringComparison.OrdinalIgnoreCase))
            {
                return header.Value;
            }
        }

        return null;
    }
}
