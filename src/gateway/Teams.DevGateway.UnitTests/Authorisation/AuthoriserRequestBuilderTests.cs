using Teams.DevGateway.Authorisation;

namespace Teams.DevGateway.UnitTests.Authorisation;

public class AuthoriserRequestBuilderTests
{
    [Fact]
    public void Build_includes_the_authorization_header_when_present()
    {
        var request = AuthoriserRequestBuilder.Build("Bearer abc", "/users/self", "GET");

        Assert.Equal("REQUEST", request.Type);
        Assert.Equal("/users/self", request.Path);
        Assert.Equal("GET", request.HttpMethod);
        Assert.Equal("Bearer abc", request.Headers["Authorization"]);
    }

    [Fact]
    public void Build_produces_an_empty_headers_dictionary_when_authorization_is_missing()
    {
        var request = AuthoriserRequestBuilder.Build(null, "/users/self", "GET");

        Assert.Empty(request.Headers);
    }
}