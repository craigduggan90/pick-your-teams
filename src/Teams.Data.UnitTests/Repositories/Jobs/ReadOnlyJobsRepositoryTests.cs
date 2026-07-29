using Teams.Data.Models;
using Teams.Data.Repositories.Jobs;
using Teams.Domain.Enums;

namespace Teams.Data.UnitTests.Repositories.Jobs;

public static class ReadOnlyJobsRepositoryTests
{
    public class GetByIdAsync : JobsRepositoryTestsBase
    {
        private ReadOnlyJobsRepository CreateSut() => new(Context);

        [Fact]
        public async Task ShouldReturnNull_WhenRecordDoesNotExist()
        {
            const string id = "000";
            var sut = CreateSut();
            var actual = await sut.GetByIdAsync(id, TestContext.Current.CancellationToken);
            Assert.Null(actual);
        }

        [Fact]
        public async Task ShouldReturnEntity_WhenRecordExists()
        {
            const string id = "001";
            var expected = await Context.Jobs.FindAsync([id], TestContext.Current.CancellationToken);

            var sut = CreateSut();
            var actual = await sut.GetByIdAsync(id, TestContext.Current.CancellationToken);
            Assert.Equivalent(expected, actual, true);
        }
    }

    public class GetByIdempotencyKeyAsync : JobsRepositoryTestsBase
    {
        private ReadOnlyJobsRepository CreateSut() => new(Context);

        [Fact]
        public async Task ShouldReturnNull_WhenRecordDoesNotExist()
        {
            const string idempotencyKey = "does-not-exist";
            var sut = CreateSut();
            var actual = await sut.GetByIdempotencyKeyAsync(idempotencyKey, TestContext.Current.CancellationToken);
            Assert.Null(actual);
        }

        [Fact]
        public async Task ShouldReturnEntity_WhenRecordExists()
        {
            const string idempotencyKey = "idempotency-key-001";
            var expected = Context.Jobs.Single(j => j.IdempotencyKey == idempotencyKey);

            var sut = CreateSut();
            var actual = await sut.GetByIdempotencyKeyAsync(idempotencyKey, TestContext.Current.CancellationToken);
            Assert.Equivalent(expected, actual, true);
        }
    }

    public class GetAsync : JobsRepositoryTestsBase
    {
        private ReadOnlyJobsRepository CreateSut() => new(Context);

        [Fact]
        public async Task ShouldReturnFirstPage_WhenNoParametersProvided()
        {
            var firstPage = Context.Jobs.OrderBy(j => j.Cursor).Take(Constants.DefaultPageSize);
            var sut = CreateSut();
            var actual = await sut.GetAsync(cancellationToken: TestContext.Current.CancellationToken);
            Assert.Equivalent(firstPage, actual, true);
        }

        [Fact]
        public async Task ShouldReturnFilteredPage_WhenTypeProvided()
        {
            const JobTypeEnum type = JobTypeEnum.ArchiveUserJob;
            var expected = Context.Jobs.Where(j => j.Type == type)
                .OrderBy(j => j.Cursor)
                .Take(Constants.DefaultPageSize);

            var sut = CreateSut();
            var actual = await sut.GetAsync(type: type, cancellationToken: TestContext.Current.CancellationToken);

            Assert.Equal(10, actual.Count());
            Assert.Equivalent(expected, actual, true);
        }

        [Fact]
        public async Task ShouldReturnFilteredPage_WhenStatusProvided()
        {
            const JobStatusEnum status = JobStatusEnum.Failed;
            var expected = Context.Jobs.Where(j => j.Status == status)
                .OrderBy(j => j.Cursor)
                .Take(Constants.DefaultPageSize);

            var sut = CreateSut();
            var actual = await sut.GetAsync(status: status, cancellationToken: TestContext.Current.CancellationToken);

            Assert.Equivalent(expected, actual, true);
        }

        [Fact]
        public async Task ShouldReturnFilteredPage_WhenErrorCodeProvided()
        {
            const string errorCode = "ERR-004";
            var expected = Context.Jobs.Where(j => j.ErrorCode == errorCode)
                .OrderBy(j => j.Cursor);

            var sut = CreateSut();
            var actual = await sut.GetAsync(errorCode: errorCode, cancellationToken: TestContext.Current.CancellationToken);

            Assert.Equivalent(expected, actual, true);
        }

        [Fact]
        public async Task ShouldReturnFilteredPage_WhenCreatedFromProvided()
        {
            var createdFrom = BaseDate.AddDays(10);
            var expected = Context.Jobs.Where(j => j.DateCreated >= createdFrom)
                .OrderBy(j => j.Cursor)
                .Take(Constants.DefaultPageSize);

            var sut = CreateSut();
            var actual = await sut.GetAsync(
                dateFilter: new DateFilter(CreatedFrom: createdFrom, CreatedTo: null, ModifiedFrom: null, ModifiedTo: null),
                cancellationToken: TestContext.Current.CancellationToken);

            Assert.Equivalent(expected, actual, true);
        }

        [Fact]
        public async Task ShouldReturnFilteredPage_WhenCreatedToProvided()
        {
            var createdTo = BaseDate.AddDays(20);
            var expected = Context.Jobs.Where(j => j.DateCreated < createdTo)
                .OrderBy(j => j.Cursor);

            var sut = CreateSut();
            var actual = await sut.GetAsync(
                dateFilter: new DateFilter(CreatedFrom: null, CreatedTo: createdTo, ModifiedFrom: null, ModifiedTo: null),
                cancellationToken: TestContext.Current.CancellationToken);

            Assert.Equivalent(expected, actual, true);
        }

        [Fact]
        public async Task ShouldReturnFilteredPage_WhenModifiedFromProvided()
        {
            var modifiedFrom = BaseDate.AddDays(10).AddYears(1);
            var expected = Context.Jobs.Where(j => j.DateModified >= modifiedFrom)
                .OrderBy(j => j.Cursor)
                .Take(Constants.DefaultPageSize);

            var sut = CreateSut();
            var actual = await sut.GetAsync(
                dateFilter: new DateFilter(CreatedFrom: null, CreatedTo: null, ModifiedFrom: modifiedFrom, ModifiedTo: null),
                cancellationToken: TestContext.Current.CancellationToken);

            Assert.Equivalent(expected, actual, true);
        }

        [Fact]
        public async Task ShouldReturnFilteredPage_WhenModifiedToProvided()
        {
            var modifiedTo = BaseDate.AddDays(15).AddYears(1);
            var expected = Context.Jobs.Where(j => j.DateModified < modifiedTo)
                .OrderBy(j => j.Cursor);

            var sut = CreateSut();
            var actual = await sut.GetAsync(
                dateFilter: new DateFilter(CreatedFrom: null, CreatedTo: null, ModifiedFrom: null, ModifiedTo: modifiedTo),
                cancellationToken: TestContext.Current.CancellationToken);

            Assert.Equivalent(expected, actual, true);
        }

        [Fact]
        public async Task ShouldReturnSpecifiedPage_WhenPaginationFilterProvided()
        {
            var page = Context.Jobs.OrderBy(j => j.Cursor).Skip(5).Take(5);
            var cursor = Context.Jobs.OrderBy(j => j.Cursor).Skip(4).First().Cursor;

            var sut = CreateSut();
            var actual = await sut.GetAsync(
                pagination: new PaginationFilter(cursor, 5),
                cancellationToken: TestContext.Current.CancellationToken);

            Assert.Equivalent(page, actual, true);
        }
    }
}