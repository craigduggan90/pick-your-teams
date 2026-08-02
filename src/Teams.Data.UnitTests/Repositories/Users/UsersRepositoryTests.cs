using Microsoft.EntityFrameworkCore;
using Teams.Data.Repositories.Users;
using Teams.Domain.Entities;

namespace Teams.Data.UnitTests.Repositories.Users;

public static class UsersRepositoryTests
{
    public class CreateAsync : RepositoryTestBase
    {
        private UsersRepository CreateSut() => new(Context);

        [Fact]
        public async Task Should_AddEntity_ToChangeTracker()
        {
            var entity = SeedDataFactory.Users.Create(1000);
            var sut = CreateSut();
            _ = await sut.CreateAsync(entity, TestContext.Current.CancellationToken);

            var tracked = Context.ChangeTracker.Entries<User>()
                .Single(entry => entry.State == EntityState.Added)
                .Entity;

            Assert.Same(entity, tracked);
        }
    }

    public class UpdateAsync : RepositoryTestBase
    {
        private UsersRepository CreateSut() => new(Context);

        [Fact]
        public async Task Should_UpdateEntity_InChangeTracker()
        {
            var entity = Context.Users.Skip(7).First();
            entity.Update("New Value", null, null);

            var sut = CreateSut();
            _ = await sut.UpdateAsync(entity, TestContext.Current.CancellationToken);

            var tracked = Context.ChangeTracker.Entries<User>()
                .Single(entry => entry.State == EntityState.Modified)
                .Entity;

            Assert.Same(entity, tracked);
        }
    }

    public class DeleteAsync : RepositoryTestBase
    {
        private UsersRepository CreateSut() => new(Context);

        [Fact]
        public async Task Should_DeleteEntity_InChangeTracker()
        {
            var entity = Context.Users.Skip(8).First();

            var sut = CreateSut();
            _ = await sut.DeleteAsync(entity, TestContext.Current.CancellationToken);

            var tracked = Context.ChangeTracker.Entries<User>()
                .Single(entry => entry.State == EntityState.Deleted)
                .Entity;

            Assert.Same(entity, tracked);
        }
    }
}