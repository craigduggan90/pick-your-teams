using System.Text.Json;

namespace Teams.Authoriser.Auth;

public interface IJwksClient
{
    Task<JsonElement> GetJwksAsync(CancellationToken cancellationToken);
}
