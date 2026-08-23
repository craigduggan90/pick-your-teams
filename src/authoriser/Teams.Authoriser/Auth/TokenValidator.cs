using System.Security.Cryptography.X509Certificates;

namespace Teams.Authoriser.Auth;

public enum TokenValidationOutcome
{
    MissingOrMalformed,
    SignatureInvalid,
    Valid,
}

public record TokenValidationResult(TokenValidationOutcome Outcome, string? Subject = null, DateTime? ExpiresAtUtc = null);

/// <summary>
/// Orchestrates the real validation path: parse → fetch JWKS → match `kid` → verify signature.
/// Only validates the token itself - turning a valid token into a resolved Teams.Api user is
/// UserResolver's job, not this class's.
/// </summary>
public class TokenValidator(IJwksClient jwksClient, string issuer, string audience)
{
    public async Task<TokenValidationResult> ValidateAsync(string? authorizationHeader, CancellationToken cancellationToken)
    {
        if (!BearerTokenParser.TryGetBearerToken(authorizationHeader, out var token)
            || !JwtReader.TryReadJwt(token, out var jwt)
            || jwt is null
            || string.IsNullOrEmpty(jwt.Header.Kid)
            || string.IsNullOrEmpty(jwt.Subject))
        {
            return new TokenValidationResult(TokenValidationOutcome.MissingOrMalformed);
        }

        byte[]? certificateBytes;
        try
        {
            var jwks = await jwksClient.GetJwksAsync(cancellationToken);
            certificateBytes = JwksKeySelector.FindSigningCertificate(jwks, jwt.Header.Kid);
        }
        catch
        {
            return new TokenValidationResult(TokenValidationOutcome.SignatureInvalid);
        }

        if (certificateBytes is null)
        {
            return new TokenValidationResult(TokenValidationOutcome.SignatureInvalid);
        }

        var signingCertificate = X509CertificateLoader.LoadCertificate(certificateBytes);
        var isValid = JwtSignatureValidator.IsValid(token, signingCertificate, issuer, audience);

        return isValid
            ? new TokenValidationResult(TokenValidationOutcome.Valid, jwt.Subject, jwt.ValidTo)
            : new TokenValidationResult(TokenValidationOutcome.SignatureInvalid);
    }
}