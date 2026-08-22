using System.Net;
using Teams.Authoriser.Auth;

namespace Teams.Authoriser.UnitTests.Auth;

public class Auth0JwksClientTests
{
    private const string Domain = "example-tenant.us.auth0.com";

    private sealed class FakeHttpMessageHandler(IReadOnlyDictionary<string, string> responsesByUrl) : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var url = request.RequestUri!.ToString();

            if (!responsesByUrl.TryGetValue(url, out var body))
            {
                return Task.FromResult(new HttpResponseMessage(HttpStatusCode.NotFound));
            }

            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(body, System.Text.Encoding.UTF8, "application/json"),
            });
        }
    }

    [Fact]
    public async Task GetJwksAsync_follows_discovery_document_to_jwks_uri()
    {
        var jwksUri = $"https://{Domain}/.well-known/jwks.json";
        var responses = new Dictionary<string, string>
        {
            [$"https://{Domain}/.well-known/openid-configuration"] = $$"""{ "jwks_uri": "{{jwksUri}}" }""",
            [jwksUri] = """{ "keys": [ { "kid": "abc" } ] }""",
        };

        var client = new Auth0JwksClient(new HttpClient(new FakeHttpMessageHandler(responses)), Domain);

        var jwks = await client.GetJwksAsync(CancellationToken.None);

        Assert.Equal("abc", jwks.GetProperty("keys")[0].GetProperty("kid").GetString());
    }

    [Fact]
    public async Task GetJwksAsync_throws_when_discovery_document_has_no_jwks_uri()
    {
        var responses = new Dictionary<string, string>
        {
            [$"https://{Domain}/.well-known/openid-configuration"] = "{}",
        };

        var client = new Auth0JwksClient(new HttpClient(new FakeHttpMessageHandler(responses)), Domain);

        await Assert.ThrowsAsync<InvalidOperationException>(() => client.GetJwksAsync(CancellationToken.None));
    }
}
