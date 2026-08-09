using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using System.Net.Http.Json;
using System.Text.Json;
using Teams.Common;
using Teams.Domain.Entities;

namespace Teams.Api.IntegrationTests;

public abstract class ApiControllerTestsBase(ApiWebApplicationFactory factory)
    : IClassFixture<ApiWebApplicationFactory>, IDisposable
{
    private const string UserIdHeader = "Teams-User-Id";
    private const string UserTagHeader = "Teams-User-Tag";
    private const string UserNameHeader = "Teams-User-Name";

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

    /// <summary>Extracts the validation error messages for a given field from the problem details "errors" extension.</summary>
    protected static IReadOnlyList<string> GetValidationErrors(ProblemDetails problemDetails, string propertyName) =>
        ((JsonElement)problemDetails.Extensions["errors"]!)
            .GetProperty(propertyName)
            .EnumerateArray()
            .Select(error => error.GetString()!)
            .ToList();

    /// <summary>Extracts every validation error message from the problem details "errors" extension, regardless of
    /// which field it's keyed under - useful for RuleForEach failures, whose keys include an indexer (e.g.
    /// "AwayTeamIds[0]") that callers can't predict.</summary>
    protected static IReadOnlyList<string> GetAllValidationErrors(ProblemDetails problemDetails) =>
        ((JsonElement)problemDetails.Extensions["errors"]!)
            .EnumerateObject()
            .SelectMany(property => property.Value.EnumerateArray().Select(error => error.GetString()!))
            .ToList();

    protected static IEnumerable<string> GetHeaderValues(HttpResponseMessage response, string headerName) =>
        response.Headers.TryGetValues(headerName, out var values) ? values : [];

    protected static string ToETagValue(string concurrencyToken) => $"\"{concurrencyToken}\"";

    /// <summary>Attaches the actor headers required by any endpoint that resolves <c>IActorAccessor.Current</c>.</summary>
    protected static HttpRequestMessage WithActorHeaders(HttpRequestMessage request, User actor) =>
        WithActorHeaders(request, actor.Id, actor.Tag, actor.DisplayName);

    /// <summary>Attaches the actor headers required by any endpoint that resolves <c>IActorAccessor.Current</c>.</summary>
    protected static HttpRequestMessage WithActorHeaders(HttpRequestMessage request, string id, string tag, string displayName)
    {
        request.Headers.Add(UserIdHeader, id);
        request.Headers.Add(UserTagHeader, tag);
        request.Headers.Add(UserNameHeader, displayName);
        return request;
    }

    public void Dispose()
    {
        Client.Dispose();
        GC.SuppressFinalize(this);
    }
}