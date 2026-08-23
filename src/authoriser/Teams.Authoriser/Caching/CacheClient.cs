using Microsoft.Extensions.Caching.Memory;

namespace Teams.Authoriser.Caching;

public class CacheClient(IMemoryCache cache) : ICacheClient
{
    public async Task<T?> GetOrCreateAsync<T>(string key, Func<Task<T?>> factory, TimeSpan expiration)
        where T : class
    {
        if (cache.TryGetValue(key, out T? cached))
        {
            return cached;
        }

        var value = await factory();
        if (value is not null)
        {
            cache.Set(key, value, expiration);
        }

        return value;
    }
}