using Amazon.Lambda.APIGatewayEvents;

namespace Teams.Authoriser.Auth;

/// <summary>
/// Builds real AWS-shaped authorizer responses (IAM policy documents). Uses
/// APIGatewayCustomAuthorizerV2IamResponse rather than the V1 response type - both carry the same
/// PrincipalID/PolicyDocument shape, but V2's Context is a real Dictionary&lt;string, object&gt;,
/// where V1's is a fixed 3-property placeholder that can't carry named keys like Teams-User-Id.
/// </summary>
public static class AuthorizerPolicyFactory
{
    public static APIGatewayCustomAuthorizerV2IamResponse Deny(string? methodArn) =>
        BuildResponse("unauthorized", "Deny", methodArn, context: null);

    public static APIGatewayCustomAuthorizerV2IamResponse Allow(string? methodArn, ResolvedUser user) =>
        BuildResponse(
            user.Id,
            "Allow",
            methodArn,
            new Dictionary<string, object>
            {
                ["Teams-User-Id"] = user.Id,
                ["Teams-User-Tag"] = user.Tag,
                ["Teams-User-Name"] = user.DisplayName,
                // No end-user request is ever granted the authoriser scope - only this
                // component's own direct calls to Teams.Api carry it. Empty here, always.
                ["Scopes"] = "",
            });

    private static APIGatewayCustomAuthorizerV2IamResponse BuildResponse(
        string principalId, string effect, string? methodArn, Dictionary<string, object>? context) => new()
        {
            PrincipalID = principalId,
            PolicyDocument = new APIGatewayCustomAuthorizerPolicy
            {
                Version = "2012-10-17",
                Statement =
            [
                new APIGatewayCustomAuthorizerPolicy.IAMPolicyStatement
                {
                    Effect = effect,
                    Action = ["execute-api:Invoke"],
                    Resource = [methodArn ?? "*"],
                },
            ],
            },
            Context = context,
        };
}