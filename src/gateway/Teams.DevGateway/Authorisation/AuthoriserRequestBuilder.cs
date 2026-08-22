using Amazon.Lambda.APIGatewayEvents;

namespace Teams.DevGateway.Authorisation;

/// <summary>Builds the same request shape API Gateway would send a REQUEST-type custom authorizer.</summary>
public static class AuthoriserRequestBuilder
{
    public static APIGatewayCustomAuthorizerRequest Build(string? authorizationHeader, string path, string httpMethod) => new()
    {
        Type = "REQUEST",
        Path = path,
        HttpMethod = httpMethod,
        Headers = authorizationHeader is null
            ? new Dictionary<string, string>()
            : new Dictionary<string, string> { ["Authorization"] = authorizationHeader },
    };
}
