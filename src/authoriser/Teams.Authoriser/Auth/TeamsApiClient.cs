using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

namespace Teams.Authoriser.Auth;

/// <summary>
/// Calls the two Teams.Api endpoints reserved for the authoriser (GetUserByExternalId, CreateUser)
/// - both require the Scopes: authoriser header, which only this component is meant to send.
/// </summary>
public class TeamsApiClient(HttpClient httpClient) : ITeamsApiClient
{
    private const string ScopeHeaderKey = "Scopes";
    private const string AuthoriserScope = "authoriser";
    private const string UsersPath = "/api/v1/users";

    // Teams.Api's own convention: response bodies are camelCase, request bodies stay PascalCase
    // (matching the C# DTOs directly). JsonContent.Create defaults to camelCase for everything -
    // RequestOptions overrides that back to PascalCase for the one request body this client sends.
    private static readonly JsonSerializerOptions ResponseOptions = new(JsonSerializerDefaults.Web);
    private static readonly JsonSerializerOptions RequestOptions = new() { PropertyNamingPolicy = null };

    private sealed record CreateUserRequestBody(string DisplayName, string ExternalId, string Email, string? Mobile);

    public async Task<TeamsUser?> GetByExternalIdAsync(string externalId, CancellationToken cancellationToken)
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, $"{UsersPath}/external/{Uri.EscapeDataString(externalId)}");
        request.Headers.Add(ScopeHeaderKey, AuthoriserScope);

        using var response = await httpClient.SendAsync(request, cancellationToken);

        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            return null;
        }

        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<TeamsUser>(ResponseOptions, cancellationToken);
    }

    public async Task<TeamsUser> CreateAsync(string displayName, string externalId, string email, CancellationToken cancellationToken)
    {
        using var request = new HttpRequestMessage(HttpMethod.Post, UsersPath)
        {
            Content = JsonContent.Create(new CreateUserRequestBody(displayName, externalId, email, Mobile: null), options: RequestOptions),
        };
        request.Headers.Add(ScopeHeaderKey, AuthoriserScope);

        using var response = await httpClient.SendAsync(request, cancellationToken);
        response.EnsureSuccessStatusCode();

        return (await response.Content.ReadFromJsonAsync<TeamsUser>(ResponseOptions, cancellationToken))!;
    }
}