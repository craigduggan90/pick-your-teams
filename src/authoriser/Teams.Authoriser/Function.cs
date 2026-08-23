using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using Microsoft.Extensions.Caching.Memory;
using Teams.Authoriser.Auth;
using Teams.Authoriser.Caching;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace Teams.Authoriser;

/// <summary>
/// Local-dev stand-in for the production Lambda authorizer described in claude.md's Auth model
/// section. Validates the caller's Auth0-issued token for real (JWKS signature check against the
/// dev tenant, via <see cref="Auth.TokenValidator"/>), then resolves it to a Teams.Api user (via
/// <see cref="Auth.UserResolver"/>) - looking one up by external id, or creating one from the
/// token's own Auth0 /userinfo profile on first login. The resolved user is cached per access
/// token (see <see cref="Caching.ICacheClient"/>) so repeat requests with the same still-live
/// token don't hit Teams.Api/Auth0 again. Any failure along the way denies. Deliberately thin and
/// not unit tested itself - all of the real logic lives in the Auth/ and Caching/ modules, which
/// are. This class is instantiated once and reused across warm Lambda invocations (the same is
/// true of Teams.Authoriser.LocalHost's single Function instance), which is what makes the
/// instance-level cache field actually useful rather than pointless.
/// </summary>
public class Function
{
    private const string Auth0Domain = "dev-e1zjkp6ynw1uag2f.us.auth0.com";
    private const string Audience = "http://localhost:5199";
    private const string TeamsApiBaseUrl = "http://localhost:5199";

    private readonly TokenValidator tokenValidator = new(
        new Auth0JwksClient(new HttpClient(), Auth0Domain),
        issuer: $"https://{Auth0Domain}/",
        audience: Audience);

    private readonly UserResolver userResolver = new(
        new TeamsApiClient(new HttpClient { BaseAddress = new Uri(TeamsApiBaseUrl) }),
        new Auth0UserInfoClient(new HttpClient(), Auth0Domain));

    private readonly ICacheClient cacheClient = new MemoryCacheClient(new MemoryCache(new MemoryCacheOptions()));

    public async Task<APIGatewayCustomAuthorizerV2IamResponse> FunctionHandler(
        APIGatewayCustomAuthorizerRequest request, ILambdaContext context)
    {
        var authorizationHeader = AuthorizationHeaderReader.GetAuthorizationHeader(request.Headers);
        var result = await tokenValidator.ValidateAsync(authorizationHeader, CancellationToken.None);

        if (result.Outcome != TokenValidationOutcome.Valid || result.Subject is null || result.ExpiresAtUtc is null)
        {
            context.Logger.LogInformation(result.Outcome == TokenValidationOutcome.MissingOrMalformed
                ? "Denying: missing or malformed bearer token."
                : "Denying: JWT signature/claims failed validation.");
            return AuthorizerPolicyFactory.Deny(request.MethodArn);
        }

        BearerTokenParser.TryGetBearerToken(authorizationHeader, out var accessToken);
        var cacheExpiry = CacheExpiryCalculator.Calculate(result.ExpiresAtUtc.Value, DateTime.UtcNow);
        var resolvedUser = await cacheClient.GetOrCreateAsync(
            accessToken,
            () => userResolver.ResolveAsync(result.Subject, accessToken, CancellationToken.None),
            cacheExpiry);

        if (resolvedUser is null)
        {
            context.Logger.LogInformation($"Denying: could not resolve or create a user for sub={result.Subject}.");
            return AuthorizerPolicyFactory.Deny(request.MethodArn);
        }

        context.Logger.LogInformation($"Allowing: resolved user {resolvedUser.Id} for sub={result.Subject}.");
        return AuthorizerPolicyFactory.Allow(request.MethodArn, resolvedUser);
    }
}