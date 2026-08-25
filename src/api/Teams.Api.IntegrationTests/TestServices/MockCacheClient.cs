using Teams.Common.Extensions;
using Teams.Core.Services;

namespace Teams.Api.IntegrationTests.TestServices;

public class MockCacheClient : ICacheClient
{
    private static readonly TimeSpan DefaultExpiration = TimeSpan.FromMinutes(15);

    public Dictionary<string, string> Values { get; } = [];

    public Task<T> GetOrCreateAsync<T>(string key, Func<Task<T>> factory) =>
        GetOrCreateAsync(key, factory, DefaultExpiration);

    public async Task<T> GetOrCreateAsync<T>(string key, Func<Task<T>> factory, TimeSpan expiration)
    {
        if (Values.TryGetValue(key, out var cached))
            return cached.Deserialize<T>()!;

        var value = await factory();
        Values[key] = value.Serialize();
        return value;
    }

    public Task ExpireAsync(string key)
    {
        Values.Remove(key);
        return Task.CompletedTask;
    }
}