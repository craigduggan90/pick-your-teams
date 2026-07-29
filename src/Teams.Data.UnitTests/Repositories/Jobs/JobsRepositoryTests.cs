using Microsoft.EntityFrameworkCore;
using Teams.Domain.Entities;
using Teams.Data.Repositories.Jobs;
using Teams.Domain.Enums;

namespace Teams.Data.UnitTests.Repositories.Jobs;

public static class JobsRepositoryTests
{
    public class CreateAsync : JobsRepositoryTestsBase
    {
        private JobsRepository CreateSut() => new(Context);

        [Fact]
        public async Task Should_AddEntity_ToChangeTracker()
        {
            var entity = new Job("idempotency-key-new", JobTypeEnum.ArchiveProjectJob, null);
            var sut = CreateSut();
            _ = await sut.CreateAsync(entity, TestContext.Current.CancellationToken);

            var tracked = Context.ChangeTracker.Entries<Job>()
                .Single(entry => entry.State == EntityState.Added)
                .Entity;

            Assert.Same(entity, tracked);
        }
    }

    public class UpdateAsync : JobsRepositoryTestsBase
    {
        private JobsRepository CreateSut() => new(Context);

        [Fact]
        public async Task Should_UpdateEntity_InChangeTracker()
        {
            var entity = Context.Jobs.Skip(15).First();
            entity.Update(JobStatusEnum.InProgress, null, null);

            var sut = CreateSut();
            _ = await sut.UpdateAsync(entity, TestContext.Current.CancellationToken);

            var tracked = Context.ChangeTracker.Entries<Job>()
                .Single(entry => entry.State == EntityState.Modified)
                .Entity;

            Assert.Same(entity, tracked);
        }
    }

    public class DeleteAsync : JobsRepositoryTestsBase
    {
        private JobsRepository CreateSut() => new(Context);

        [Fact]
        public async Task Should_DeleteEntity_InChangeTracker()
        {
            var entity = Context.Jobs.Skip(15).First();

            var sut = CreateSut();
            _ = await sut.DeleteAsync(entity, TestContext.Current.CancellationToken);

            var tracked = Context.ChangeTracker.Entries<Job>()
                .Single(entry => entry.State == EntityState.Deleted)
                .Entity;

            Assert.Same(entity, tracked);
        }
    }
}