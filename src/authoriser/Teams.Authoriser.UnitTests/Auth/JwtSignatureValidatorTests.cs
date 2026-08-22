using Teams.Authoriser.Auth;
using Teams.Authoriser.UnitTests.TestHelpers;

namespace Teams.Authoriser.UnitTests.Auth;

public class JwtSignatureValidatorTests
{
    private const string Issuer = "https://issuer.example/";
    private const string Audience = "https://api.example/";

    [Fact]
    public void IsValid_accepts_a_correctly_signed_token()
    {
        var (certificate, key) = TestTokenFactory.CreateSigningCertificate();
        var token = TestTokenFactory.CreateSignedJwt(key, "kid-1", Issuer, Audience, "user-123");

        Assert.True(JwtSignatureValidator.IsValid(token, certificate, Issuer, Audience));
    }

    [Fact]
    public void IsValid_rejects_a_token_signed_by_a_different_key()
    {
        var (_, signingKey) = TestTokenFactory.CreateSigningCertificate();
        var (wrongCertificate, _) = TestTokenFactory.CreateSigningCertificate();
        var token = TestTokenFactory.CreateSignedJwt(signingKey, "kid-1", Issuer, Audience, "user-123");

        Assert.False(JwtSignatureValidator.IsValid(token, wrongCertificate, Issuer, Audience));
    }

    [Fact]
    public void IsValid_rejects_an_expired_token()
    {
        var (certificate, key) = TestTokenFactory.CreateSigningCertificate();
        var token = TestTokenFactory.CreateSignedJwt(
            key, "kid-1", Issuer, Audience, "user-123", expires: DateTime.UtcNow.AddHours(-1));

        Assert.False(JwtSignatureValidator.IsValid(token, certificate, Issuer, Audience));
    }

    [Fact]
    public void IsValid_rejects_a_token_with_the_wrong_audience()
    {
        var (certificate, key) = TestTokenFactory.CreateSigningCertificate();
        var token = TestTokenFactory.CreateSignedJwt(key, "kid-1", Issuer, "wrong-audience", "user-123");

        Assert.False(JwtSignatureValidator.IsValid(token, certificate, Issuer, Audience));
    }

    [Fact]
    public void IsValid_rejects_a_token_with_the_wrong_issuer()
    {
        var (certificate, key) = TestTokenFactory.CreateSigningCertificate();
        var token = TestTokenFactory.CreateSignedJwt(key, "kid-1", "https://wrong-issuer.example/", Audience, "user-123");

        Assert.False(JwtSignatureValidator.IsValid(token, certificate, Issuer, Audience));
    }
}
