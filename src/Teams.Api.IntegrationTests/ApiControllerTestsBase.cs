using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using Teams.Common;
using System.Net.Http.Json;

namespace Teams.Api.IntegrationTests;

public abstract class ApiControllerTestsBase(ApiWebApplicationFactory factory)
    : IClassFixture<ApiWebApplicationFactory>, IDisposable
{
    protected HttpClient Client { get; } = factory.CreateClient();

    protected ApiWebApplicationFactory Factory { get; } = factory;

    protected static HttpRequestMessage CreateRequest(
        HttpMethod method,
        string requestUri,
        string? scopes = null,
        string? apiVersion = null,
        HttpContent? content = null)
    {
        var request = new HttpRequestMessage(method, requestUri) { Content = content };

        if (scopes is not null)
            request.Headers.Add(Constants.ScopeHeaderKey, scopes);

        if (apiVersion is not null)
            request.Headers.Add(Constants.ApiVersionHeaderKey, apiVersion);

        return request;
    }

    protected static HttpRequestMessage CreateJsonRequest<T>(
        HttpMethod method,
        string requestUri,
        T body,
        string? scopes = null,
        string? apiVersion = null) =>
        CreateRequest(method, requestUri, scopes, apiVersion, JsonContent.Create(body));

    protected static string WithQuery(string requestUri, params (string Key, string? Value)[] parameters)
    {
        var query = parameters
            .Where(p => p.Value is not null)
            .ToDictionary(p => p.Key, p => p.Value);

        return QueryHelpers.AddQueryString(requestUri, query);
    }

    protected static Task<T?> ReadContentAsync<T>(HttpResponseMessage response, CancellationToken cancellationToken) =>
        response.Content.ReadFromJsonAsync<T>(cancellationToken);

    protected static Task<ProblemDetails?> ReadProblemDetailsAsync(HttpResponseMessage response, CancellationToken cancellationToken) =>
        ReadContentAsync<ProblemDetails>(response, cancellationToken);

    protected static IEnumerable<string> GetHeaderValues(HttpResponseMessage response, string headerName) =>
        response.Headers.TryGetValues(headerName, out var values) ? values : [];

    protected static string ToETagValue(string concurrencyToken) => $"\"{concurrencyToken}\"";

    public void Dispose()
    {
        Client.Dispose();
        GC.SuppressFinalize(this);
    }
}