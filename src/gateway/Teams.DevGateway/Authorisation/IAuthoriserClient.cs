using Amazon.Lambda.APIGatewayEvents;

namespace Teams.DevGateway.Authorisation;

public interface IAuthoriserClient
{
    Task<APIGatewayCustomAuthorizerV2IamResponse> AuthorizeAsync(
        APIGatewayCustomAuthorizerRequest request, CancellationToken cancellationToken);
}