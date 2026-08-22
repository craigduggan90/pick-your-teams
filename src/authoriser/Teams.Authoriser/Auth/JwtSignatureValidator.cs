using System.IdentityModel.Tokens.Jwt;
using System.Security.Cryptography.X509Certificates;
using Microsoft.IdentityModel.Tokens;

namespace Teams.Authoriser.Auth;

/// <summary>Validates a JWT's signature, issuer, audience, and lifetime against a signing certificate.</summary>
public static class JwtSignatureValidator
{
    public static bool IsValid(string token, X509Certificate2 signingCertificate, string issuer, string audience)
    {
        var validationParameters = new TokenValidationParameters
        {
            ValidIssuer = issuer,
            ValidAudience = audience,
            IssuerSigningKey = new X509SecurityKey(signingCertificate),
        };

        try
        {
            new JwtSecurityTokenHandler().ValidateToken(token, validationParameters, out _);
            return true;
        }
        catch (SecurityTokenException)
        {
            return false;
        }
    }
}
