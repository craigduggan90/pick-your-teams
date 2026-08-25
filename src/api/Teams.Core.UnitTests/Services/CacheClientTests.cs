using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using Teams.Core.Services;

namespace Teams.Core.UnitTests.Services;

public static class CacheClientTests
{
    private sealed record CachedValue(string Value);

    public class GetOrCreateAsync
    {
        private static CacheClient CreateSut() => new(new MemoryDistributedCache(Options.Create(new MemoryDistributedCacheOptions())));

        [Fact]
        public async Task ShouldReturnFactoryResult_WhenKeyIsNotCached()
        {
            var sut = CreateSut();

            var result = await sut.GetOrCreateAsync("key", () => Task.FromResult(new CachedValue("first")), TimeSpan.FromMinutes(1));

            Assert.Equal("first", result.Value);
        }

        [Fact]
        public async Task ShouldReturnCachedValue_WithoutCallingFactoryAgain_WhenKeyIsCached()
        {
            var sut = CreateSut();
            var factoryCalls = 0;

            Task<CachedValue> Factory()
            {
                factoryCalls++;
                return Task.FromResult(new CachedValue($"call-{factoryCalls}"));
            }

            var first = await sut.GetOrCreateAsync("key", Factory, TimeSpan.FromMinutes(1));
            var second = await sut.GetOrCreateAsync("key", Factory, TimeSpan.FromMinutes(1));

            Assert.Equal(1, factoryCalls);
            Assert.Equal(first, second);
        }

        [Fact]
        public async Task ShouldNotShareEntries_AcrossDifferentKeys()
        {
            var sut = CreateSut();

            var a = await sut.GetOrCreateAsync("key-a", () => Task.FromResult(new CachedValue("a")), TimeSpan.FromMinutes(1));
            var b = await sut.GetOrCreateAsync("key-b", () => Task.FromResult(new CachedValue("b")), TimeSpan.FromMinutes(1));

            Assert.Equal("a", a.Value);
            Assert.Equal("b", b.Value);
        }

        [Fact]
        public async Task ShouldReturnFactoryResult_WhenCalledWithoutAnExplicitExpiration()
        {
            var sut = CreateSut();

            var result = await sut.GetOrCreateAsync("key", () => Task.FromResult(new CachedValue("first")));

            Assert.Equal("first", result.Value);
        }

        [Fact]
        public async Task ShouldShareCacheEntries_BetweenTheDefaultAndExplicitExpirationOverloads()
        {
            var sut = CreateSut();
            var factoryCalls = 0;

            Task<CachedValue> Factory()
            {
                factoryCalls++;
                return Task.FromResult(new CachedValue($"call-{factoryCalls}"));
            }

            await sut.GetOrCreateAsync("key", Factory);
            await sut.GetOrCreateAsync("key", Factory, TimeSpan.FromMinutes(1));

            Assert.Equal(1, factoryCalls);
        }
    }

    public class ExpireAsync
    {
        private static CacheClient CreateSut() => new(new MemoryDistributedCache(Options.Create(new MemoryDistributedCacheOptions())));

        [Fact]
        public async Task ShouldCauseTheNextGetOrCreateAsyncCall_ToRunTheFactoryAgain()
        {
            var sut = CreateSut();
            var factoryCalls = 0;

            Task<CachedValue> Factory()
            {
                factoryCalls++;
                return Task.FromResult(new CachedValue($"call-{factoryCalls}"));
            }

            await sut.GetOrCreateAsync("key", Factory, TimeSpan.FromMinutes(1));
            await sut.ExpireAsync("key");
            await sut.GetOrCreateAsync("key", Factory, TimeSpan.FromMinutes(1));

            Assert.Equal(2, factoryCalls);
        }
    }
}