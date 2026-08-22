using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using Teams.Authoriser.Auth;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace Teams.Authoriser;

/// <summary>
/// Local-dev stand-in for the production Lambda authorizer described in claude.md's Auth model
/// section. Validates the caller's Auth0-issued token for real (JWKS signature check against the
/// dev tenant, via <see cref="Auth.TokenValidator"/>) but always denies for now: turning a
/// verified token into a resolved User is blocked on a Teams.Api endpoint that doesn't exist yet
/// (see the TODO below). Deliberately thin and not unit tested itself — all of the real logic
/// lives in the Auth/ modules, which are.
/// </summary>
public class Function
{
    private const string Auth0Domain = "dev-e1zjkp6ynw1uag2f.us.auth0.com";
    private const string Audience = "http://localhost:5199";

    private readonly TokenValidator tokenValidator = new(
        new Auth0JwksClient(new HttpClient(), Auth0Domain),
        issuer: $"https://{Auth0Domain}/",
        audience: Audience);

    public async Task<APIGatewayCustomAuthorizerResponse> FunctionHandler(
        APIGatewayCustomAuthorizerRequest request, ILambdaContext context)
    {
        var authorizationHeader = AuthorizationHeaderReader.GetAuthorizationHeader(request.Headers);
        var result = await tokenValidator.ValidateAsync(authorizationHeader, CancellationToken.None);

        switch (result.Outcome)
        {
            case TokenValidationOutcome.MissingOrMalformed:
                context.Logger.LogInformation("Denying: missing or malformed bearer token.");
                break;
            case TokenValidationOutcome.SignatureInvalid:
                context.Logger.LogInformation("Denying: JWT signature/claims failed validation.");
                break;
            case TokenValidationOutcome.Valid:
                // TODO: look the user up by external id (result.Subject, the JWT 'sub' claim)
                // once Teams.Api exposes a GetByExternalId lookup; if it returns null, create the
                // user for that external id (see Teams.Core's CreateUserCommand). Until then a
                // verified token still denies, and the resolved user's Id/Tag/DisplayName should
                // be returned via the response's Context so Teams.DevGateway can turn them into
                // the Teams-User-* headers Teams.Api requires.
                context.Logger.LogInformation(
                    $"Token verified for sub={result.Subject}, but user resolution isn't built yet — denying.");
                break;
        }

        return AuthorizerPolicyFactory.Deny(request.MethodArn);
    }
}