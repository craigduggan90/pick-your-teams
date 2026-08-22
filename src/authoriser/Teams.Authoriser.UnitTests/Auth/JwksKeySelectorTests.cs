using System.Text.Json;
using Teams.Authoriser.Auth;

namespace Teams.Authoriser.UnitTests.Auth;

public class JwksKeySelectorTests
{
    private static JsonElement ParseJwks(string json) => JsonDocument.Parse(json).RootElement;

    [Fact]
    public void FindSigningCertificate_returns_matching_certificate_bytes()
    {
        var jwks = ParseJwks("""
        {
            "keys": [
                { "kid": "other-kid", "x5c": ["b3RoZXI="] },
                { "kid": "target-kid", "x5c": ["dGFyZ2V0"] }
            ]
        }
        """);

        var result = JwksKeySelector.FindSigningCertificate(jwks, "target-kid");

        Assert.Equal(Convert.FromBase64String("dGFyZ2V0"), result);
    }

    [Fact]
    public void FindSigningCertificate_returns_null_when_no_kid_matches()
    {
        var jwks = ParseJwks("""
        { "keys": [ { "kid": "other-kid", "x5c": ["b3RoZXI="] } ] }
        """);

        Assert.Null(JwksKeySelector.FindSigningCertificate(jwks, "missing-kid"));
    }

    [Fact]
    public void FindSigningCertificate_ignores_keys_without_x5c()
    {
        var jwks = ParseJwks("""
        { "keys": [ { "kid": "target-kid" } ] }
        """);

        Assert.Null(JwksKeySelector.FindSigningCertificate(jwks, "target-kid"));
    }

    [Fact]
    public void FindSigningCertificate_ignores_keys_with_empty_x5c()
    {
        var jwks = ParseJwks("""
        { "keys": [ { "kid": "target-kid", "x5c": [] } ] }
        """);

        Assert.Null(JwksKeySelector.FindSigningCertificate(jwks, "target-kid"));
    }

    [Fact]
    public void FindSigningCertificate_returns_null_when_no_keys_property()
    {
        var jwks = ParseJwks("{}");

        Assert.Null(JwksKeySelector.FindSigningCertificate(jwks, "any-kid"));
    }
}