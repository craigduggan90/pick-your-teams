using Amazon.Lambda.APIGatewayEvents;
using System.Text.Json;

namespace Teams.DevGateway.Authorisation;

/// <summary>Reads the Effect and (on Allow) the resolved user out of an authorizer's real
/// AWS-shaped IAM policy response.</summary>
public static class AuthorisationResponseInterpreter
{
    public static bool IsAllowed(APIGatewayCustomAuthorizerV2IamResponse response) =>
        response.PolicyDocument?.Statement?.Any(
            statement => string.Equals(statement.Effect, "Allow", StringComparison.OrdinalIgnoreCase)) ?? false;

    /// <summary>Context values round-trip through JSON as boxed JsonElement, not plain strings.</summary>
    public static string? GetContextValue(APIGatewayCustomAuthorizerV2IamResponse response, string key)
    {
        if (response.Context is null || !response.Context.TryGetValue(key, out var value))
        {
            return null;
        }

        return value switch
        {
            JsonElement element => element.ValueKind == JsonValueKind.String ? element.GetString() : null,
            string stringValue => stringValue,
            _ => null,
        };
    }
}