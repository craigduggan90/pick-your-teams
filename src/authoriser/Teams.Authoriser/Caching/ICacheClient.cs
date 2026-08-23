namespace Teams.Authoriser.Caching;

/// <summary>
/// Deliberately not IDistributedCache - that's a real design decision to revisit if/when this
/// moves to Redis, not something to abstract over prematurely. For now, a plain in-memory
/// implementation is all that's needed.
/// </summary>
public interface ICacheClient
{
    /// <summary>
    /// Returns the cached value for <paramref name="key"/> if present; otherwise calls
    /// <paramref name="factory"/>. If the factory returns null, nothing is cached and null is
    /// returned. Otherwise the factory's result is cached for <paramref name="expiration"/> and
    /// returned.
    /// </summary>
    Task<T?> GetOrCreateAsync<T>(string key, Func<Task<T?>> factory, TimeSpan expiration)
        where T : class;
}