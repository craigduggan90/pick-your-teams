namespace Teams.Authoriser.Auth;

public interface ITeamsApiClient
{
    /// <summary>Null when Teams.Api returns 404. Throws for any other non-success response.</summary>
    Task<TeamsUser?> GetByExternalIdAsync(string externalId, CancellationToken cancellationToken);

    /// <summary>Throws for any non-success response (e.g. 422 validation failure).</summary>
    Task<TeamsUser> CreateAsync(string displayName, string externalId, string email, CancellationToken cancellationToken);
}