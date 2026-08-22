using System.IdentityModel.Tokens.Jwt;

namespace Teams.Authoriser.Auth;

/// <summary>Structural-only JWT parsing: three segments, decodable header/payload. No signature check.</summary>
public static class JwtReader
{
    public static bool TryReadJwt(string token, out JwtSecurityToken? jwt)
    {
        var handler = new JwtSecurityTokenHandler();
        jwt = null;

        if (!handler.CanReadToken(token))
        {
            return false;
        }

        try
        {
            jwt = handler.ReadJwtToken(token);
            return true;
        }
        catch (ArgumentException)
        {
            return false;
        }
    }
}