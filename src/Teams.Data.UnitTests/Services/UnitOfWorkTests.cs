using Microsoft.EntityFrameworkCore;
using Teams.Common.Providers.Temporal;
using Teams.Data.Context;
using Teams.Data.Services;
using Teams.Data.UnitTests.TestHelpers;
using Teams.Domain.Entities;

namespace Teams.Data.UnitTests.Services;

public static class UnitOfWorkTests
{
    public class Players : DatabaseAwareTestBase
    {
        [Fact]
        public void Should_ReturnsRepository_WhenCalledForTheFirstTime()
        {
            var sut = CreateSut(Context);
            var repository = sut.Players;
            Assert.NotNull(repository);
        }

        [Fact]
        public void Should_ReusesRepository_ForSubsequentCalls()
        {
            var sut = CreateSut(Context);
            var firstRepository = sut.Players;
            var secondRepository = sut.Players;
            Assert.Same(firstRepository, secondRepository);
        }
    }

    public class Games : DatabaseAwareTestBase
    {
        [Fact]
        public void Should_ReturnsRepository_WhenCalledForTheFirstTime()
        {
            var sut = CreateSut(Context);
            var repository = sut.Games;
            Assert.NotNull(repository);
        }

        [Fact]
        public void Should_ReusesRepository_ForSubsequentCalls()
        {
            var sut = CreateSut(Context);
            var firstRepository = sut.Games;
            var secondRepository = sut.Games;
            Assert.Same(firstRepository, secondRepository);
        }
    }

    public class Users : DatabaseAwareTestBase
    {
        [Fact]
        public void Should_ReturnsRepository_WhenCalledForTheFirstTime()
        {
            var sut = CreateSut(Context);
            var repository = sut.Users;
            Assert.NotNull(repository);
        }

        [Fact]
        public void Should_ReusesRepository_ForSubsequentCalls()
        {
            var sut = CreateSut(Context);
            var firstRepository = sut.Users;
            var secondRepository = sut.Users;
            Assert.Same(firstRepository, secondRepository);
        }
    }

    public class SaveChangesAsync : DatabaseAwareTestBase
    {
        [Fact]
        public async Task Should_CommitChanges()
        {
            var sut = CreateSut(Context);
            await sut.Games.CreateAsync(new Game(null, DateTimeOffsetProvider.Now.UtcDateTime, 60, 5), TestContext.Current.CancellationToken);

            var initialEntry = Assert.Single(Context.ChangeTracker.Entries());
            Assert.Equal(EntityState.Added, initialEntry.State);

            await sut.SaveChangesAsync(CancellationToken.None);

            // Added changes to Unchanged once saved
            var postSaveEntry = Assert.Single(Context.ChangeTracker.Entries());
            Assert.Equal(EntityState.Unchanged, postSaveEntry.State);
        }
    }

    public class Dispose : DatabaseAwareTestBase
    {
        [Fact]
        public void Should_DisposeContext_IfInitialized()
        {
            var sut = CreateSut(Context);

            // Call this to make sut initialize the context.
            _ = sut.Players;

            // Disposing the sut should dispose the initialized context (in this case `Context`)
            sut.Dispose();

            // Trying to work with the context after disposal should throw ObjectDisposedException
            Assert.Throws<ObjectDisposedException>(() => Context.SaveChanges());
        }

        [Fact]
        public void Should_DoNothing_IfContextNotInitialized()
        {
            // This time we won't assign _context, so Context won't be disposed when sut is
            var sut = CreateSut(Context);
            sut.Dispose();

            // Must not throw
            Context.SaveChanges();
        }
    }

    public class DisposeAsync : DatabaseAwareTestBase
    {
        [Fact]
        public async Task Should_DisposeContext_IfInitialized()
        {
            var sut = CreateSut(Context);

            // Call this to make sut initialize the context
            _ = await sut.Players.GetAsync(cancellationToken: TestContext.Current.CancellationToken);

            // Disposing the sut should dispose the initialized context (in this case `Context`)
            await sut.DisposeAsync();

            // Trying to work with the context after disposal should throw ObjectDisposedException
            Assert.Throws<ObjectDisposedException>(() => Context.SaveChanges());
        }

        [Fact]
        public async Task Should_DoNothing_IfContextNotInitialized()
        {
            // This time we won't assign _context, so Context won't be disposed when sut is
            var sut = CreateSut(Context);
            await sut.DisposeAsync();

            await Context.SaveChangesAsync(TestContext.Current.CancellationToken);
        }
    }

    private static UnitOfWork CreateSut(ApiDbContext context)
        => new(GetDbContextFactory(context));

    private static IApiDbContextFactory GetDbContextFactory(ApiDbContext context)
    {
        var factory = Substitute.For<IApiDbContextFactory>();
        factory.CreateDbContext(ContextType.ReadWrite).Returns(context);
        return factory;
    }
}