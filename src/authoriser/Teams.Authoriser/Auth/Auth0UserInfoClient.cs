using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace Teams.Authoriser.Auth;

/// <summary>
/// Calls Auth0's /userinfo with the same access token the caller already presented - not M2M,
/// just re-presenting the user's own token to fetch the profile claims (name, email) needed to
/// create a User on first login. Works regardless of the token's aud (a custom API identifier
/// here, not Auth0's own userinfo audience) as long as it carries openid scope, which it does by
/// default.
/// </summary>
public class Auth0UserInfoClient(HttpClient httpClient, string domain) : IUserInfoClient
{
    public async Task<UserInfo?> GetUserInfoAsync(string accessToken, CancellationToken cancellationToken)
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, $"https://{domain}/userinfo");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

        using var response = await httpClient.SendAsync(request, cancellationToken);

        return response.IsSuccessStatusCode
            ? await response.Content.ReadFromJsonAsync<UserInfo>(cancellationToken)
            : null;
    }
}