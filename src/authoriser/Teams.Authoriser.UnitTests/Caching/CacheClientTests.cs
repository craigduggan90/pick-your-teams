using Microsoft.Extensions.Caching.Memory;
using Teams.Authoriser.Caching;

namespace Teams.Authoriser.UnitTests.Caching;

public class CacheClientTests
{
    private sealed record CachedValue(string Value);

    private static CacheClient CreateSut() => new(new MemoryCache(new MemoryCacheOptions()));

    [Fact]
    public async Task GetOrCreateAsync_calls_the_factory_and_returns_its_result_on_a_miss()
    {
        var sut = CreateSut();

        var result = await sut.GetOrCreateAsync("key", () => Task.FromResult<CachedValue?>(new CachedValue("first")), TimeSpan.FromMinutes(1));

        Assert.Equal("first", result?.Value);
    }

    [Fact]
    public async Task GetOrCreateAsync_returns_the_cached_value_without_calling_the_factory_again()
    {
        var sut = CreateSut();
        var factoryCalls = 0;

        Task<CachedValue?> Factory()
        {
            factoryCalls++;
            return Task.FromResult<CachedValue?>(new CachedValue($"call-{factoryCalls}"));
        }

        var first = await sut.GetOrCreateAsync("key", Factory, TimeSpan.FromMinutes(1));
        var second = await sut.GetOrCreateAsync("key", Factory, TimeSpan.FromMinutes(1));

        Assert.Equal(1, factoryCalls);
        Assert.Same(first, second);
    }

    [Fact]
    public async Task GetOrCreateAsync_does_not_cache_a_null_factory_result()
    {
        var sut = CreateSut();
        var factoryCalls = 0;

        Task<CachedValue?> Factory()
        {
            factoryCalls++;
            return Task.FromResult<CachedValue?>(null);
        }

        var first = await sut.GetOrCreateAsync("key", Factory, TimeSpan.FromMinutes(1));
        var second = await sut.GetOrCreateAsync("key", Factory, TimeSpan.FromMinutes(1));

        Assert.Null(first);
        Assert.Null(second);
        Assert.Equal(2, factoryCalls);
    }

    [Fact]
    public async Task GetOrCreateAsync_does_not_share_entries_across_different_keys()
    {
        var sut = CreateSut();

        var a = await sut.GetOrCreateAsync("key-a", () => Task.FromResult<CachedValue?>(new CachedValue("a")), TimeSpan.FromMinutes(1));
        var b = await sut.GetOrCreateAsync("key-b", () => Task.FromResult<CachedValue?>(new CachedValue("b")), TimeSpan.FromMinutes(1));

        Assert.Equal("a", a?.Value);
        Assert.Equal("b", b?.Value);
    }
}