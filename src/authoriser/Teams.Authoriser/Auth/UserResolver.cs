namespace Teams.Authoriser.Auth;

public record ResolvedUser(string Id, string Tag, string DisplayName);

/// <summary>
/// Look up a user by external id; if none exists, create one from the token's own /userinfo
/// profile. Any failure along the way - a lookup error, missing profile claims, a failed create -
/// resolves to null, which the caller treats as Deny. This is deliberately fail-closed: there's no
/// partial-success state where an unresolved user still gets through.
/// </summary>
public class UserResolver(ITeamsApiClient teamsApiClient, IUserInfoClient userInfoClient)
{
    public async Task<ResolvedUser?> ResolveAsync(string externalId, string accessToken, CancellationToken cancellationToken)
    {
        try
        {
            var existing = await teamsApiClient.GetByExternalIdAsync(externalId, cancellationToken);
            if (existing is not null)
            {
                return new ResolvedUser(existing.Id, existing.Tag, existing.DisplayName);
            }

            var userInfo = await userInfoClient.GetUserInfoAsync(accessToken, cancellationToken);
            if (string.IsNullOrWhiteSpace(userInfo?.Name) || string.IsNullOrWhiteSpace(userInfo?.Email))
            {
                return null;
            }

            var created = await teamsApiClient.CreateAsync(userInfo.Name, externalId, userInfo.Email, cancellationToken);
            return new ResolvedUser(created.Id, created.Tag, created.DisplayName);
        }
        catch
        {
            return null;
        }
    }
}