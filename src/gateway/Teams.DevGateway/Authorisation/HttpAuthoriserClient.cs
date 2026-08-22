using System.Net.Http.Json;
using Amazon.Lambda.APIGatewayEvents;

namespace Teams.DevGateway.Authorisation;

/// <summary>Calls Teams.Authoriser.LocalHost, the plain HTTP host that invokes the real Lambda
/// authorizer handler for us — see that repo's README for why it exists.</summary>
public class HttpAuthoriserClient(HttpClient httpClient) : IAuthoriserClient
{
    public async Task<APIGatewayCustomAuthorizerResponse> AuthorizeAsync(
        APIGatewayCustomAuthorizerRequest request, CancellationToken cancellationToken)
    {
        var httpResponse = await httpClient.PostAsJsonAsync("/authorize", request, cancellationToken);
        httpResponse.EnsureSuccessStatusCode();

        return (await httpResponse.Content.ReadFromJsonAsync<APIGatewayCustomAuthorizerResponse>(cancellationToken))!;
    }
}
