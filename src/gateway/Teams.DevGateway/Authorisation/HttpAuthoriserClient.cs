using Amazon.Lambda.APIGatewayEvents;

namespace Teams.DevGateway.Authorisation;

/// <summary>Calls Teams.Authoriser.LocalHost, the plain HTTP host that invokes the real Lambda
/// authorizer handler for us — see that repo's README for why it exists.</summary>
public class HttpAuthoriserClient(HttpClient httpClient) : IAuthoriserClient
{
    public async Task<APIGatewayCustomAuthorizerV2IamResponse> AuthorizeAsync(
        APIGatewayCustomAuthorizerRequest request, CancellationToken cancellationToken)
    {
        var httpResponse = await httpClient.PostAsJsonAsync("/authorize", request, cancellationToken);
        httpResponse.EnsureSuccessStatusCode();

        return (await httpResponse.Content.ReadFromJsonAsync<APIGatewayCustomAuthorizerV2IamResponse>(cancellationToken))!;
    }
}