using System.Text.Json;

namespace Teams.Authoriser.Auth;

/// <summary>
/// Pure lookup over an already-fetched JWKS document: finds the DER certificate bytes (from
/// `x5c`) for the signing key matching a given `kid`. No I/O.
/// </summary>
public static class JwksKeySelector
{
    public static byte[]? FindSigningCertificate(JsonElement jwks, string kid)
    {
        if (!jwks.TryGetProperty("keys", out var keys))
        {
            return null;
        }

        foreach (var key in keys.EnumerateArray())
        {
            var hasMatchingKid = key.TryGetProperty("kid", out var keyKid) && keyKid.GetString() == kid;
            var hasSigningCertificate = key.TryGetProperty("x5c", out var x5c) && x5c.GetArrayLength() > 0;

            if (!hasMatchingKid || !hasSigningCertificate)
            {
                continue;
            }

            return Convert.FromBase64String(x5c.EnumerateArray().First().GetString()!);
        }

        return null;
    }
}
