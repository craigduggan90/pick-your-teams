using Teams.Authoriser.Auth;

namespace Teams.Authoriser.UnitTests.Auth;

public class AuthorizationHeaderReaderTests
{
    [Fact]
    public void GetAuthorizationHeader_returns_null_when_headers_is_null()
    {
        Assert.Null(AuthorizationHeaderReader.GetAuthorizationHeader(null));
    }

    [Fact]
    public void GetAuthorizationHeader_returns_null_when_header_missing()
    {
        var headers = new Dictionary<string, string> { ["Content-Type"] = "application/json" };

        Assert.Null(AuthorizationHeaderReader.GetAuthorizationHeader(headers));
    }

    [Fact]
    public void GetAuthorizationHeader_returns_value_for_exact_case()
    {
        var headers = new Dictionary<string, string> { ["Authorization"] = "Bearer abc" };

        Assert.Equal("Bearer abc", AuthorizationHeaderReader.GetAuthorizationHeader(headers));
    }

    [Theory]
    [InlineData("authorization")]
    [InlineData("AUTHORIZATION")]
    [InlineData("AuthOrization")]
    public void GetAuthorizationHeader_is_case_insensitive(string headerName)
    {
        var headers = new Dictionary<string, string> { [headerName] = "Bearer abc" };

        Assert.Equal("Bearer abc", AuthorizationHeaderReader.GetAuthorizationHeader(headers));
    }
}