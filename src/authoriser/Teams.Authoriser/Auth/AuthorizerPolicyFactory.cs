using Amazon.Lambda.APIGatewayEvents;

namespace Teams.Authoriser.Auth;

/// <summary>Builds real AWS-shaped authorizer responses (IAM policy documents).</summary>
public static class AuthorizerPolicyFactory
{
    public static APIGatewayCustomAuthorizerResponse Deny(string? methodArn) => new()
    {
        PrincipalID = "unauthorized",
        PolicyDocument = new APIGatewayCustomAuthorizerPolicy
        {
            Version = "2012-10-17",
            Statement =
            [
                new APIGatewayCustomAuthorizerPolicy.IAMPolicyStatement
                {
                    Effect = "Deny",
                    Action = ["execute-api:Invoke"],
                    Resource = [methodArn ?? "*"],
                },
            ],
        },
    };
}
