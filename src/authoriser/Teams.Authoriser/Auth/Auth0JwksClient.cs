using System.Net.Http.Json;
using System.Text.Json;

namespace Teams.Authoriser.Auth;

/// <summary>
/// Fetches Auth0's live JWKS via its OIDC discovery document. Fetched fresh every call — no
/// caching, since this runs as a Lambda where there's little to gain from it locally.
/// </summary>
public class Auth0JwksClient(HttpClient httpClient, string domain) : IJwksClient
{
    public async Task<JsonElement> GetJwksAsync(CancellationToken cancellationToken)
    {
        var discoveryDocument = await httpClient.GetFromJsonAsync<JsonElement>(
            $"https://{domain}/.well-known/openid-configuration", cancellationToken);

        var jwksUri = discoveryDocument.TryGetProperty("jwks_uri", out var jwksUriProperty)
            ? jwksUriProperty.GetString()
            : null;

        if (jwksUri is null)
        {
            throw new InvalidOperationException(
                $"Auth0 discovery document for '{domain}' did not contain a jwks_uri.");
        }

        return await httpClient.GetFromJsonAsync<JsonElement>(jwksUri, cancellationToken);
    }
}