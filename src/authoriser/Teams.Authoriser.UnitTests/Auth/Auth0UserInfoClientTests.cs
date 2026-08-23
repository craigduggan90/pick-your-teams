using System.Net;
using System.Text;
using Teams.Authoriser.Auth;

namespace Teams.Authoriser.UnitTests.Auth;

public class Auth0UserInfoClientTests
{
    private const string Domain = "example-tenant.us.auth0.com";

    private sealed class FakeHttpMessageHandler(Func<HttpRequestMessage, HttpResponseMessage> respond) : HttpMessageHandler
    {
        public HttpRequestMessage? LastRequest { get; private set; }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            LastRequest = request;
            return Task.FromResult(respond(request));
        }
    }

    [Fact]
    public async Task GetUserInfoAsync_returns_name_and_email_on_success()
    {
        const string json = """{ "sub": "google-oauth2|1", "name": "Craig", "email": "craig@example.com", "email_verified": true }""";
        var handler = new FakeHttpMessageHandler(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(json, Encoding.UTF8, "application/json"),
        });
        var client = new Auth0UserInfoClient(new HttpClient(handler), Domain);

        var result = await client.GetUserInfoAsync("some-access-token", CancellationToken.None);

        Assert.NotNull(result);
        Assert.Equal("Craig", result.Name);
        Assert.Equal("craig@example.com", result.Email);
    }

    [Fact]
    public async Task GetUserInfoAsync_sends_the_token_as_a_bearer_credential_to_the_tenants_userinfo_endpoint()
    {
        var handler = new FakeHttpMessageHandler(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent("""{ "sub": "x" }""", Encoding.UTF8, "application/json"),
        });
        var client = new Auth0UserInfoClient(new HttpClient(handler), Domain);

        await client.GetUserInfoAsync("some-access-token", CancellationToken.None);

        Assert.Equal($"https://{Domain}/userinfo", handler.LastRequest!.RequestUri!.ToString());
        Assert.Equal("Bearer", handler.LastRequest.Headers.Authorization!.Scheme);
        Assert.Equal("some-access-token", handler.LastRequest.Headers.Authorization.Parameter);
    }

    [Fact]
    public async Task GetUserInfoAsync_returns_null_on_a_non_success_response()
    {
        var handler = new FakeHttpMessageHandler(_ => new HttpResponseMessage(HttpStatusCode.Unauthorized));
        var client = new Auth0UserInfoClient(new HttpClient(handler), Domain);

        var result = await client.GetUserInfoAsync("some-access-token", CancellationToken.None);

        Assert.Null(result);
    }
}