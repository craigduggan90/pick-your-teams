using Teams.Authoriser.Auth;
using Teams.Authoriser.UnitTests.TestHelpers;

namespace Teams.Authoriser.UnitTests.Auth;

public class JwtReaderTests
{
    [Fact]
    public void TryReadJwt_reads_a_well_formed_token()
    {
        var (_, key) = TestTokenFactory.CreateSigningCertificate();
        var token = TestTokenFactory.CreateSignedJwt(key, "kid-1", "issuer", "audience", "user-123");

        var result = JwtReader.TryReadJwt(token, out var jwt);

        Assert.True(result);
        Assert.NotNull(jwt);
        Assert.Equal("kid-1", jwt.Header.Kid);
        Assert.Equal("user-123", jwt.Subject);
    }

    [Theory]
    [InlineData("")]
    [InlineData("not-a-jwt")]
    [InlineData("only.two")]
    [InlineData("a.b.c.d")]
    [InlineData("not-base64!.not-base64!.not-base64!")]
    public void TryReadJwt_rejects_malformed_tokens(string token)
    {
        var result = JwtReader.TryReadJwt(token, out var jwt);

        Assert.False(result);
        Assert.Null(jwt);
    }
}