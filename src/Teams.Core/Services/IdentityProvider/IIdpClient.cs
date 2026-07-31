namespace Teams.Core.Services.IdentityProvider;

public interface IIdpClient
{
    Task<string> CreateUser(
        string tag,
        string? emailAddress,
        string? mobile,
        CancellationToken cancellationToken);
}