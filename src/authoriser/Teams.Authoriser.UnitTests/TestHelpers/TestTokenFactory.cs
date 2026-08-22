using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;

namespace Teams.Authoriser.UnitTests.TestHelpers;

/// <summary>Generates real, RSA-signed JWTs and matching self-signed certificates for tests
/// that exercise the actual JWKS/signature-verification code path, without any network calls.</summary>
public static class TestTokenFactory
{
    public static (X509Certificate2 Certificate, RSA PrivateKey) CreateSigningCertificate()
    {
        var rsa = RSA.Create(2048);
        var request = new CertificateRequest("CN=teams-authoriser-tests", rsa, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
        var certificate = request.CreateSelfSigned(DateTimeOffset.UtcNow.AddDays(-1), DateTimeOffset.UtcNow.AddDays(1));
        return (certificate, rsa);
    }

    public static string CreateSignedJwt(
        RSA signingKey,
        string kid,
        string issuer,
        string audience,
        string subject,
        DateTime? expires = null)
    {
        var signingCredentials = new SigningCredentials(
            new RsaSecurityKey(signingKey) { KeyId = kid },
            SecurityAlgorithms.RsaSha256);

        var token = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            claims: [new Claim(JwtRegisteredClaimNames.Sub, subject)],
            expires: expires ?? DateTime.UtcNow.AddHours(1),
            signingCredentials: signingCredentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public static string ToBase64X5c(X509Certificate2 certificate) => Convert.ToBase64String(certificate.RawData);
}