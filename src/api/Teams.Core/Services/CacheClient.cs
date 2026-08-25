using Microsoft.Extensions.Caching.Distributed;
using Teams.Common.Extensions;

namespace Teams.Core.Services;

public class CacheClient(IDistributedCache cache) : ICacheClient
{
    private static readonly TimeSpan DefaultExpiration = TimeSpan.FromMinutes(15);

    public Task<T> GetOrCreateAsync<T>(string key, Func<Task<T>> factory) =>
        GetOrCreateAsync(key, factory, DefaultExpiration);

    public async Task<T> GetOrCreateAsync<T>(string key, Func<Task<T>> factory, TimeSpan expiration)
    {
        var cached = await cache.GetStringAsync(key);
        if (cached is not null)
            return cached.Deserialize<T>()!;

        var value = await factory();

        await cache.SetStringAsync(
            key,
            value.Serialize(),
            new DistributedCacheEntryOptions { AbsoluteExpirationRelativeToNow = expiration });

        return value;
    }

    public Task ExpireAsync(string key) => cache.RemoveAsync(key);
}