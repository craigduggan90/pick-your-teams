using NSubstitute;
using System.Text.Json;
using Teams.Authoriser.Auth;
using Teams.Authoriser.UnitTests.TestHelpers;

namespace Teams.Authoriser.UnitTests.Auth;

public class TokenValidatorTests
{
    private const string Issuer = "https://issuer.example/";
    private const string Audience = "https://api.example/";

    private static JsonElement JwksContaining(string kid, string certificateBase64) =>
        JsonDocument.Parse($$"""
        { "keys": [ { "kid": "{{kid}}", "x5c": ["{{certificateBase64}}"] } ] }
        """).RootElement;

    [Fact]
    public async Task ValidateAsync_returns_Valid_for_a_correctly_signed_token()
    {
        var (certificate, key) = TestTokenFactory.CreateSigningCertificate();
        var expiresAt = new DateTime(2099, 1, 1, 12, 0, 0, DateTimeKind.Utc);
        var token = TestTokenFactory.CreateSignedJwt(key, "kid-1", Issuer, Audience, "auth0|user-123", expiresAt);

        var jwksClient = Substitute.For<IJwksClient>();
        jwksClient.GetJwksAsync(Arg.Any<CancellationToken>())
            .Returns(JwksContaining("kid-1", TestTokenFactory.ToBase64X5c(certificate)));

        var validator = new TokenValidator(jwksClient, Issuer, Audience);

        var result = await validator.ValidateAsync($"Bearer {token}", CancellationToken.None);

        Assert.Equal(TokenValidationOutcome.Valid, result.Outcome);
        Assert.Equal("auth0|user-123", result.Subject);
        Assert.Equal(expiresAt, result.ExpiresAtUtc);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("not-a-bearer-token")]
    [InlineData("Bearer not-a-jwt")]
    public async Task ValidateAsync_returns_MissingOrMalformed_without_calling_the_jwks_client(string? header)
    {
        var jwksClient = Substitute.For<IJwksClient>();
        var validator = new TokenValidator(jwksClient, Issuer, Audience);

        var result = await validator.ValidateAsync(header, CancellationToken.None);

        Assert.Equal(TokenValidationOutcome.MissingOrMalformed, result.Outcome);
        await jwksClient.DidNotReceive().GetJwksAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ValidateAsync_returns_MissingOrMalformed_without_calling_the_jwks_client_when_subject_is_missing()
    {
        var (_, key) = TestTokenFactory.CreateSigningCertificate();
        var token = TestTokenFactory.CreateSignedJwt(key, "kid-1", Issuer, Audience, subject: "");

        var jwksClient = Substitute.For<IJwksClient>();
        var validator = new TokenValidator(jwksClient, Issuer, Audience);

        var result = await validator.ValidateAsync($"Bearer {token}", CancellationToken.None);

        Assert.Equal(TokenValidationOutcome.MissingOrMalformed, result.Outcome);
        await jwksClient.DidNotReceive().GetJwksAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ValidateAsync_returns_SignatureInvalid_when_no_key_matches_the_kid()
    {
        var (_, key) = TestTokenFactory.CreateSigningCertificate();
        var token = TestTokenFactory.CreateSignedJwt(key, "kid-1", Issuer, Audience, "user-123");

        var jwksClient = Substitute.For<IJwksClient>();
        jwksClient.GetJwksAsync(Arg.Any<CancellationToken>())
            .Returns(JsonDocument.Parse("""{ "keys": [] }""").RootElement);

        var validator = new TokenValidator(jwksClient, Issuer, Audience);

        var result = await validator.ValidateAsync($"Bearer {token}", CancellationToken.None);

        Assert.Equal(TokenValidationOutcome.SignatureInvalid, result.Outcome);
    }

    [Fact]
    public async Task ValidateAsync_returns_SignatureInvalid_when_the_matching_key_does_not_verify_the_signature()
    {
        var (_, signingKey) = TestTokenFactory.CreateSigningCertificate();
        var (wrongCertificate, _) = TestTokenFactory.CreateSigningCertificate();
        var token = TestTokenFactory.CreateSignedJwt(signingKey, "kid-1", Issuer, Audience, "user-123");

        var jwksClient = Substitute.For<IJwksClient>();
        jwksClient.GetJwksAsync(Arg.Any<CancellationToken>())
            .Returns(JwksContaining("kid-1", TestTokenFactory.ToBase64X5c(wrongCertificate)));

        var validator = new TokenValidator(jwksClient, Issuer, Audience);

        var result = await validator.ValidateAsync($"Bearer {token}", CancellationToken.None);

        Assert.Equal(TokenValidationOutcome.SignatureInvalid, result.Outcome);
    }

    [Fact]
    public async Task ValidateAsync_returns_SignatureInvalid_when_the_jwks_client_throws()
    {
        var (_, key) = TestTokenFactory.CreateSigningCertificate();
        var token = TestTokenFactory.CreateSignedJwt(key, "kid-1", Issuer, Audience, "user-123");

        var jwksClient = Substitute.For<IJwksClient>();
        jwksClient.GetJwksAsync(Arg.Any<CancellationToken>())
            .Returns<JsonElement>(_ => throw new HttpRequestException("network down"));

        var validator = new TokenValidator(jwksClient, Issuer, Audience);

        var result = await validator.ValidateAsync($"Bearer {token}", CancellationToken.None);

        Assert.Equal(TokenValidationOutcome.SignatureInvalid, result.Outcome);
    }
}