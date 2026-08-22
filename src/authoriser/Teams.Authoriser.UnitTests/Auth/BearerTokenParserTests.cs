using Teams.Authoriser.Auth;

namespace Teams.Authoriser.UnitTests.Auth;

public class BearerTokenParserTests
{
    [Fact]
    public void TryGetBearerToken_extracts_token_from_well_formed_header()
    {
        var result = BearerTokenParser.TryGetBearerToken("Bearer abc.def.ghi", out var token);

        Assert.True(result);
        Assert.Equal("abc.def.ghi", token);
    }

    [Fact]
    public void TryGetBearerToken_is_case_insensitive_on_prefix()
    {
        var result = BearerTokenParser.TryGetBearerToken("bearer abc.def.ghi", out var token);

        Assert.True(result);
        Assert.Equal("abc.def.ghi", token);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("abc.def.ghi")]
    [InlineData("Basic abc.def.ghi")]
    [InlineData("Bearer ")]
    [InlineData("Bearer    ")]
    public void TryGetBearerToken_rejects_missing_or_malformed_headers(string? header)
    {
        var result = BearerTokenParser.TryGetBearerToken(header, out var token);

        Assert.False(result);
        Assert.Equal(string.Empty, token);
    }
}
