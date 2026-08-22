using System.Net;
using System.Text;
using Teams.DevGateway.Authorisation;

namespace Teams.DevGateway.UnitTests.Authorisation;

public class HttpAuthoriserClientTests
{
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
    public async Task AuthorizeAsync_posts_to_authorize_and_deserializes_the_response()
    {
        const string responseJson = """{ "principalId": "unauthorized", "policyDocument": { "Version": "2012-10-17", "Statement": [ { "Effect": "Deny", "Action": ["execute-api:Invoke"], "Resource": ["*"] } ] } }""";

        var handler = new FakeHttpMessageHandler(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(responseJson, Encoding.UTF8, "application/json"),
        });

        var httpClient = new HttpClient(handler) { BaseAddress = new Uri("http://localhost:5210") };
        var client = new HttpAuthoriserClient(httpClient);

        var request = AuthoriserRequestBuilder.Build("Bearer abc", "/users/self", "GET");
        var response = await client.AuthorizeAsync(request, CancellationToken.None);

        Assert.Equal("Deny", response.PolicyDocument.Statement[0].Effect);
        Assert.Equal(HttpMethod.Post, handler.LastRequest!.Method);
        Assert.Equal("http://localhost:5210/authorize", handler.LastRequest.RequestUri!.ToString());
    }

    [Fact]
    public async Task AuthorizeAsync_throws_on_a_non_success_response()
    {
        var handler = new FakeHttpMessageHandler(_ => new HttpResponseMessage(HttpStatusCode.InternalServerError));
        var httpClient = new HttpClient(handler) { BaseAddress = new Uri("http://localhost:5210") };
        var client = new HttpAuthoriserClient(httpClient);

        var request = AuthoriserRequestBuilder.Build("Bearer abc", "/users/self", "GET");

        await Assert.ThrowsAsync<HttpRequestException>(() => client.AuthorizeAsync(request, CancellationToken.None));
    }
}
