namespace Teams.Core.Services;

public interface ICacheClient
{
    Task<T> GetOrCreateAsync<T>(string key, Func<Task<T>> factory);

    Task<T> GetOrCreateAsync<T>(string key, Func<Task<T>> factory, TimeSpan expiration);

    Task ExpireAsync(string key);
}