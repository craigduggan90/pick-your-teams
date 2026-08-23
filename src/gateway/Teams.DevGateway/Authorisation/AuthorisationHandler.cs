namespace Teams.DevGateway.Authorisation;

public record AuthorisationDecision(bool IsAllowed, string? UserId = null, string? UserTag = null, string? UserName = null, string? Scopes = null)
{
    public static AuthorisationDecision Allow(string userId, string userTag, string userName, string scopes) =>
        new(true, userId, userTag, userName, scopes);

    public static AuthorisationDecision Deny() => new(false);
}

/// <summary>
/// The only piece of "auth logic" DevGateway has: ask Teams.Authoriser, then turn its answer into
/// a decision. It never decides anything about the token itself.
/// </summary>
public class AuthorisationHandler(IAuthoriserClient authoriserClient)
{
    public async Task<AuthorisationDecision> DecideAsync(
        string? authorizationHeader, string path, string httpMethod, CancellationToken cancellationToken)
    {
        var request = AuthoriserRequestBuilder.Build(authorizationHeader, path, httpMethod);
        var response = await authoriserClient.AuthorizeAsync(request, cancellationToken);

        if (!AuthorisationResponseInterpreter.IsAllowed(response))
        {
            return AuthorisationDecision.Deny();
        }

        var userId = AuthorisationResponseInterpreter.GetContextValue(response, "Teams-User-Id");
        var userTag = AuthorisationResponseInterpreter.GetContextValue(response, "Teams-User-Tag");
        var userName = AuthorisationResponseInterpreter.GetContextValue(response, "Teams-User-Name");
        var scopes = AuthorisationResponseInterpreter.GetContextValue(response, "Scopes") ?? "";

        // An Allow without the identity it's supposed to carry is malformed, not authorised -
        // fail safe rather than forward a request with missing/blank actor headers.
        if (userId is null || userTag is null || userName is null)
        {
            return AuthorisationDecision.Deny();
        }

        return AuthorisationDecision.Allow(userId, userTag, userName, scopes);
    }
}