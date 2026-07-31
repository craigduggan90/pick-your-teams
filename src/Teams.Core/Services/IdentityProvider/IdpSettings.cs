namespace Teams.Core.Services.IdentityProvider;

public class IdpSettings
{
    public string Domain { get; init; }
    public string Audience { get; init; }

    public string ClientId { get; init; }

    public string ClientSecret { get; init; }

    public string Connection { get; init; }
}