using Microsoft.EntityFrameworkCore;
using Teams.Data.Repositories.Invitations;
using Teams.Domain.Entities;

namespace Teams.Data.UnitTests.Repositories.Invitations;

public static class InvitationsRepositoryTests
{
    public class CreateAsync : RepositoryTestBase
    {
        private InvitationsRepository CreateSut() => new(Context);

        [Fact]
        public async Task Should_AddEntity_ToChangeTracker()
        {
            var game = Context.Games.Skip(15).First();
            var entity = SeedDataFactory.Invitations.Create(1000, game);
            var sut = CreateSut();
            _ = await sut.CreateAsync(entity, TestContext.Current.CancellationToken);

            var tracked = Context.ChangeTracker.Entries<Invitation>()
                .Single(entry => entry.State == EntityState.Added)
                .Entity;

            Assert.Same(entity, tracked);
        }
    }

    public class UpdateAsync : RepositoryTestBase
    {
        private InvitationsRepository CreateSut() => new(Context);

        [Fact]
        public async Task Should_UpdateEntity_InChangeTracker()
        {
            var entity = Context.Invitations.Skip(15).First();
            entity.Accept();

            var sut = CreateSut();
            _ = await sut.UpdateAsync(entity, TestContext.Current.CancellationToken);

            var tracked = Context.ChangeTracker.Entries<Invitation>()
                .Single(entry => entry.State == EntityState.Modified)
                .Entity;

            Assert.Same(entity, tracked);
        }
    }

    public class DeleteAsync : RepositoryTestBase
    {
        private InvitationsRepository CreateSut() => new(Context);

        [Fact]
        public async Task Should_DeleteEntity_InChangeTracker()
        {
            var entity = Context.Invitations.Skip(15).First();

            var sut = CreateSut();
            _ = await sut.DeleteAsync(entity, TestContext.Current.CancellationToken);

            var tracked = Context.ChangeTracker.Entries<Invitation>()
                .Single(entry => entry.State == EntityState.Deleted)
                .Entity;

            Assert.Same(entity, tracked);
        }
    }
}