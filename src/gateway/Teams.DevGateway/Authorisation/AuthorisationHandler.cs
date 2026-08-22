namespace Teams.DevGateway.Authorisation;

public record AuthorisationDecision(bool IsAllowed)
{
    public static AuthorisationDecision Allow() => new(true);

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

        // TODO: once Teams.Authoriser's Allow branch carries the resolved user via the response
        // Context, extract Teams-User-Id/Tag/Name here so the caller can set them on the proxied
        // request. Until then Allow is unreachable — Teams.Authoriser always denies.
        return AuthorisationResponseInterpreter.IsAllowed(response)
            ? AuthorisationDecision.Allow()
            : AuthorisationDecision.Deny();
    }
}
