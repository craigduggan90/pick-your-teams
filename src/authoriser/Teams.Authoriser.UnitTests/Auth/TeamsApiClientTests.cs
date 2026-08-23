using System.Net;
using System.Text;
using System.Text.Json;
using Teams.Authoriser.Auth;

namespace Teams.Authoriser.UnitTests.Auth;

public class TeamsApiClientTests
{
    private sealed class FakeHttpMessageHandler(Func<HttpRequestMessage, HttpResponseMessage> respond) : HttpMessageHandler
    {
        public HttpRequestMessage? LastRequest { get; private set; }
        public string? LastRequestBody { get; private set; }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            LastRequest = request;
            LastRequestBody = request.Content?.ReadAsStringAsync(cancellationToken).GetAwaiter().GetResult();
            return Task.FromResult(respond(request));
        }
    }

    private static HttpClient CreateClient(FakeHttpMessageHandler handler) =>
        new(handler) { BaseAddress = new Uri("http://localhost:5199") };

    [Fact]
    public async Task GetByExternalIdAsync_returns_null_on_404()
    {
        var handler = new FakeHttpMessageHandler(_ => new HttpResponseMessage(HttpStatusCode.NotFound));
        var client = new TeamsApiClient(CreateClient(handler));

        var result = await client.GetByExternalIdAsync("missing", CancellationToken.None);

        Assert.Null(result);
    }

    [Fact]
    public async Task GetByExternalIdAsync_deserializes_a_camelCase_response()
    {
        const string json = """{ "id": "u1", "tag": "u1", "displayName": "Jane Smith", "rating": 1042 }""";
        var handler = new FakeHttpMessageHandler(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(json, Encoding.UTF8, "application/json"),
        });
        var client = new TeamsApiClient(CreateClient(handler));

        var result = await client.GetByExternalIdAsync("external-id", CancellationToken.None);

        Assert.NotNull(result);
        Assert.Equal("u1", result.Id);
        Assert.Equal("Jane Smith", result.DisplayName);
        Assert.Equal(1042, result.Rating);
    }

    [Fact]
    public async Task GetByExternalIdAsync_sends_the_authoriser_scope_header()
    {
        var handler = new FakeHttpMessageHandler(_ => new HttpResponseMessage(HttpStatusCode.NotFound));
        var client = new TeamsApiClient(CreateClient(handler));

        await client.GetByExternalIdAsync("external-id", CancellationToken.None);

        Assert.Equal("authoriser", handler.LastRequest!.Headers.GetValues("Scopes").Single());
        Assert.EndsWith("/api/v1/users/external/external-id", handler.LastRequest.RequestUri!.ToString());
    }

    [Fact]
    public async Task GetByExternalIdAsync_throws_on_a_non_success_non_404_response()
    {
        var handler = new FakeHttpMessageHandler(_ => new HttpResponseMessage(HttpStatusCode.InternalServerError));
        var client = new TeamsApiClient(CreateClient(handler));

        await Assert.ThrowsAsync<HttpRequestException>(() => client.GetByExternalIdAsync("external-id", CancellationToken.None));
    }

    [Fact]
    public async Task CreateAsync_sends_a_PascalCase_request_body_with_the_authoriser_scope_header()
    {
        const string json = """{ "id": "u1", "tag": "u1", "displayName": "Jane Smith", "rating": 1000 }""";
        var handler = new FakeHttpMessageHandler(_ => new HttpResponseMessage(HttpStatusCode.Created)
        {
            Content = new StringContent(json, Encoding.UTF8, "application/json"),
        });
        var client = new TeamsApiClient(CreateClient(handler));

        await client.CreateAsync("Jane Smith", "external-id", "jane@example.com", CancellationToken.None);

        Assert.Equal("authoriser", handler.LastRequest!.Headers.GetValues("Scopes").Single());
        Assert.Equal(HttpMethod.Post, handler.LastRequest.Method);
        Assert.EndsWith("/api/v1/users", handler.LastRequest.RequestUri!.ToString());

        var body = JsonDocument.Parse(handler.LastRequestBody!).RootElement;
        Assert.Equal("Jane Smith", body.GetProperty("DisplayName").GetString());
        Assert.Equal("external-id", body.GetProperty("ExternalId").GetString());
        Assert.Equal("jane@example.com", body.GetProperty("Email").GetString());
    }

    [Fact]
    public async Task CreateAsync_returns_the_created_user()
    {
        const string json = """{ "id": "u1", "tag": "u1", "displayName": "Jane Smith", "rating": 1000 }""";
        var handler = new FakeHttpMessageHandler(_ => new HttpResponseMessage(HttpStatusCode.Created)
        {
            Content = new StringContent(json, Encoding.UTF8, "application/json"),
        });
        var client = new TeamsApiClient(CreateClient(handler));

        var result = await client.CreateAsync("Jane Smith", "external-id", "jane@example.com", CancellationToken.None);

        Assert.Equal("u1", result.Id);
        Assert.Equal("Jane Smith", result.DisplayName);
    }

    [Fact]
    public async Task CreateAsync_throws_on_a_non_success_response()
    {
        var handler = new FakeHttpMessageHandler(_ => new HttpResponseMessage(HttpStatusCode.UnprocessableEntity));
        var client = new TeamsApiClient(CreateClient(handler));

        await Assert.ThrowsAsync<HttpRequestException>(
            () => client.CreateAsync("Jane Smith", "external-id", "jane@example.com", CancellationToken.None));
    }
}