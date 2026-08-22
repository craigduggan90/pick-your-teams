using Amazon.Lambda.APIGatewayEvents;

namespace Teams.DevGateway.Authorisation;

/// <summary>Reads the Effect out of an authorizer's real AWS-shaped IAM policy response.</summary>
public static class AuthorisationResponseInterpreter
{
    public static bool IsAllowed(APIGatewayCustomAuthorizerResponse response) =>
        response.PolicyDocument?.Statement?.Any(
            statement => string.Equals(statement.Effect, "Allow", StringComparison.OrdinalIgnoreCase)) ?? false;
}
