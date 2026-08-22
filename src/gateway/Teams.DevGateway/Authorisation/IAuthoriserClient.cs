using Amazon.Lambda.APIGatewayEvents;

namespace Teams.DevGateway.Authorisation;

public interface IAuthoriserClient
{
    Task<APIGatewayCustomAuthorizerResponse> AuthorizeAsync(
        APIGatewayCustomAuthorizerRequest request, CancellationToken cancellationToken);
}