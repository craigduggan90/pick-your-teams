using Microsoft.EntityFrameworkCore;
using Teams.Data.Repositories.Games;
using Teams.Domain.Entities;

namespace Teams.Data.UnitTests.Repositories.Games;

public static class GamesRepositoryTests
{
    public class CreateAsync : RepositoryTestBase
    {
        private GamesRepository CreateSut() => new(Context);

        [Fact]
        public async Task Should_AddEntity_ToChangeTracker()
        {
            var entity = SeedDataFactory.Games.Create(1000, GetUser(4));
            var sut = CreateSut();
            _ = await sut.CreateAsync(entity, TestContext.Current.CancellationToken);

            var tracked = Context.ChangeTracker.Entries<Game>()
                .Single(entry => entry.State == EntityState.Added)
                .Entity;

            Assert.Same(entity, tracked);
        }
    }

    public class UpdateAsync : RepositoryTestBase
    {
        private GamesRepository CreateSut() => new(Context);

        [Fact]
        public async Task Should_UpdateEntity_InChangeTracker()
        {
            var entity = Context.Games.Skip(15).First();
            entity.Update("New Value", null, null);

            var sut = CreateSut();
            _ = await sut.UpdateAsync(entity, TestContext.Current.CancellationToken);

            var tracked = Context.ChangeTracker.Entries<Game>()
                .Single(entry => entry.State == EntityState.Modified)
                .Entity;

            Assert.Same(entity, tracked);
        }
    }

    public class DeleteAsync : RepositoryTestBase
    {
        private GamesRepository CreateSut() => new(Context);

        [Fact]
        public async Task Should_DeleteEntity_InChangeTracker()
        {
            var entity = Context.Games.Skip(15).First();

            var sut = CreateSut();
            _ = await sut.DeleteAsync(entity, TestContext.Current.CancellationToken);

            var tracked = Context.ChangeTracker.Entries<Game>()
                .Single(entry => entry.State == EntityState.Deleted)
                .Entity;

            Assert.Same(entity, tracked);
        }
    }
}