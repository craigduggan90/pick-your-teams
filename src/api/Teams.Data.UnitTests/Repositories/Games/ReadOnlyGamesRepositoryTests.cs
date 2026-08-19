using Microsoft.EntityFrameworkCore;
using Teams.Data.Models;
using Teams.Data.Repositories.Games;
using Teams.Domain.Enums;

namespace Teams.Data.UnitTests.Repositories.Games;

public static class ReadOnlyGamesRepositoryTests
{
    public class GetByIdAsync : RepositoryTestBase
    {
        private ReadOnlyGamesRepository CreateSut() => new(Context);

        [Fact]
        public async Task ShouldReturnNull_WhenRecordDoesNotExist()
        {
            const string id = "does-not-exist";
            var sut = CreateSut();
            var actual = await sut.GetByIdAsync(id, TestContext.Current.CancellationToken);
            Assert.Null(actual);
        }

        [Fact]
        public async Task ShouldReturnEntity_WhenRecordExists()
        {
            var id = SeedDataFactory.Games.GetIdentifier(1);
            var expected = await Context.Games.FindAsync([id], TestContext.Current.CancellationToken);

            var sut = CreateSut();
            var actual = await sut.GetByIdAsync(id, TestContext.Current.CancellationToken);
            Assert.Equivalent(expected, actual, true);
        }

        [Fact]
        public async Task ShouldReturnGameWithNullOrganiser_WhenOrganiserHasBeenHardDeleted()
        {
            var organiser = SeedDataFactory.Users.Create(200);
            var game = SeedDataFactory.Games.Create(200, organiser);
            await Context.Users.AddAsync(organiser, TestContext.Current.CancellationToken);
            await Context.Games.AddAsync(game, TestContext.Current.CancellationToken);
            await Context.SaveChangesAsync(TestContext.Current.CancellationToken);

            // Bulk delete bypasses change tracking, so this exercises the database's own ON DELETE SET NULL
            // behavior rather than EF's client-side fixup - clearing the tracker afterward forces a genuinely
            // fresh read, rather than returning the already-tracked (now stale) Game instance from memory.
            await Context.Users.IgnoreQueryFilters().Where(u => u.Id == organiser.Id)
                .ExecuteDeleteAsync(TestContext.Current.CancellationToken);
            Context.ChangeTracker.Clear();

            var sut = CreateSut();
            var actual = await sut.GetByIdAsync(game.Id, TestContext.Current.CancellationToken);

            Assert.NotNull(actual);
            Assert.Null(actual.OrganiserId);
            Assert.Null(actual.Organiser);
        }
    }

    public class GetAsync : RepositoryTestBase
    {
        private ReadOnlyGamesRepository CreateSut() => new(Context);

        [Fact]
        public async Task ShouldReturnFirstPage_WhenNoParametersProvided()
        {
            var firstPage = Context.Games.OrderBy(g => g.Cursor).Take(Constants.DefaultPageSize);
            var sut = CreateSut();
            var actual = await sut.GetAsync(cancellationToken: TestContext.Current.CancellationToken);
            Assert.Equivalent(firstPage, actual, true);
        }

        [Fact]
        public async Task ShouldReturnFilteredPage_WhenLocationProvided()
        {
            // Isolates "Outer Space" only - "Outdoor" does not contain "Outer" as a substring.
            const string value = "Outer";
            var expected = Context.Games.Where(g => g.Location != null && g.Location.Contains(value))
                .OrderBy(g => g.Cursor)
                .Take(Constants.DefaultPageSize);

            var sut = CreateSut();
            var actual = await sut.GetAsync(location: value, cancellationToken: TestContext.Current.CancellationToken);

            Assert.Equal(Constants.DefaultPageSize, actual.Count());
            Assert.Equivalent(expected, actual, true);
        }

        [Fact]
        public async Task ShouldReturnFilteredPage_WhenStartTimeFromProvided()
        {
            var value = Context.Games.OrderBy(g => g.StartTime).Skip(59).First().StartTime;
            var expected = Context.Games.Where(g => g.StartTime >= value)
                .OrderBy(g => g.Cursor)
                .Take(Constants.DefaultPageSize);

            var sut = CreateSut();
            var actual = await sut.GetAsync(
                startTime: new RangeFilter<DateTime>(value, null),
                cancellationToken: TestContext.Current.CancellationToken);

            Assert.Equivalent(expected, actual, true);
        }

        [Fact]
        public async Task ShouldReturnFilteredPage_WhenStartTimeToProvided()
        {
            var value = Context.Games.OrderBy(g => g.StartTime).Skip(59).First().StartTime;
            var expected = Context.Games.Where(g => g.StartTime < value)
                .OrderBy(g => g.Cursor)
                .Take(Constants.DefaultPageSize);

            var sut = CreateSut();
            var actual = await sut.GetAsync(
                startTime: new RangeFilter<DateTime>(null, value),
                cancellationToken: TestContext.Current.CancellationToken);

            Assert.Equivalent(expected, actual, true);
        }

        [Fact]
        public async Task ShouldReturnFilteredPage_WhenDurationProvided()
        {
            var expected = Context.Games.Where(g => g.Duration >= 30 && g.Duration < 90)
                .OrderBy(g => g.Cursor)
                .Take(Constants.DefaultPageSize);

            var sut = CreateSut();
            var actual = await sut.GetAsync(
                duration: new RangeFilter<int>(30, 90),
                cancellationToken: TestContext.Current.CancellationToken);

            Assert.Equivalent(expected, actual, true);
        }

        [Fact]
        public async Task ShouldReturnFilteredPage_WhenTeamSizeProvided()
        {
            const int value = 7;
            var expected = Context.Games.Where(g => g.TeamSize == value)
                .OrderBy(g => g.Cursor)
                .Take(Constants.DefaultPageSize);

            var sut = CreateSut();
            var actual = await sut.GetAsync(teamSize: value, cancellationToken: TestContext.Current.CancellationToken);

            Assert.Equivalent(expected, actual, true);
        }

        [Fact]
        public async Task ShouldReturnFilteredPage_WhenStatusProvided()
        {
            const GameStatusEnum value = GameStatusEnum.Finished;
            var expected = Context.Games.Where(g => g.Status == value)
                .OrderBy(g => g.Cursor)
                .Take(Constants.DefaultPageSize);

            var sut = CreateSut();
            var actual = await sut.GetAsync(status: value, cancellationToken: TestContext.Current.CancellationToken);

            Assert.Equivalent(expected, actual, true);
        }

        [Fact]
        public async Task ShouldReturnFilteredPage_WhenOrganiserIdProvided()
        {
            var value = GetUser(5).Id;
            var expected = Context.Games.Where(g => g.OrganiserId == value)
                .OrderBy(g => g.Cursor)
                .Take(Constants.DefaultPageSize);

            var sut = CreateSut();
            var actual = await sut.GetAsync(organiserId: value, cancellationToken: TestContext.Current.CancellationToken);

            Assert.Equal(Constants.DefaultPageSize, actual.Count());
            Assert.Equivalent(expected, actual, true);
        }

        [Fact]
        public async Task ShouldReturnFilteredPage_WhenUserIdProvided()
        {
            var value = GetUser(5).Id;
            var expected = Context.Games.Where(g => g.Players.Any(p => p.UserId == value))
                .OrderBy(g => g.Cursor)
                .Take(Constants.DefaultPageSize);

            var sut = CreateSut();
            var actual = await sut.GetAsync(userId: value, cancellationToken: TestContext.Current.CancellationToken);

            Assert.Equal(Constants.DefaultPageSize, actual.Count());
            Assert.Equivalent(expected, actual, true);
        }

        [Fact]
        public async Task ShouldReturnFilteredPage_WhenCreatedFromProvided()
        {
            var value = Context.Games.OrderBy(g => g.DateCreated).Skip(89).First().DateCreated;
            var expected = Context.Games.Where(g => g.DateCreated >= value)
                .OrderBy(g => g.Cursor)
                .Take(Constants.DefaultPageSize);

            var sut = CreateSut();
            var actual = await sut.GetAsync(
                dateFilter: new DateFilter(new RangeFilter<DateTime>(value, null), null),
                cancellationToken: TestContext.Current.CancellationToken);

            Assert.Equivalent(expected, actual, true);
        }

        [Fact]
        public async Task ShouldReturnFilteredPage_WhenCreatedToProvided()
        {
            var value = Context.Games.OrderBy(g => g.DateCreated).Skip(19).First().DateCreated;
            var expected = Context.Games.Where(g => g.DateCreated < value)
                .OrderBy(g => g.Cursor)
                .Take(Constants.DefaultPageSize);

            var sut = CreateSut();
            var actual = await sut.GetAsync(
                dateFilter: new DateFilter(new RangeFilter<DateTime>(null, value), null),
                cancellationToken: TestContext.Current.CancellationToken);

            Assert.Equivalent(expected, actual, true);
        }

        [Fact]
        public async Task ShouldReturnFilteredPage_WhenModifiedFromProvided()
        {
            var value = Context.Games.OrderBy(g => g.DateModified).Skip(89).First().DateModified;
            var expected = Context.Games.Where(g => g.DateModified >= value)
                .OrderBy(g => g.Cursor)
                .Take(Constants.DefaultPageSize);

            var sut = CreateSut();
            var actual = await sut.GetAsync(
                dateFilter: new DateFilter(null, new RangeFilter<DateTime>(value, null)),
                cancellationToken: TestContext.Current.CancellationToken);

            Assert.Equivalent(expected, actual, true);
        }

        [Fact]
        public async Task ShouldReturnFilteredPage_WhenModifiedToProvided()
        {
            var value = Context.Games.OrderBy(g => g.DateModified).Skip(19).First().DateModified;
            var expected = Context.Games.Where(g => g.DateModified < value)
                .OrderBy(g => g.Cursor)
                .Take(Constants.DefaultPageSize);

            var sut = CreateSut();
            var actual = await sut.GetAsync(
                dateFilter: new DateFilter(null, new RangeFilter<DateTime>(null, value)),
                cancellationToken: TestContext.Current.CancellationToken);

            Assert.Equivalent(expected, actual, true);
        }

        [Fact]
        public async Task ShouldReturnSpecifiedPage_WhenPaginationFilterProvided()
        {
            var page = Context.Games.OrderBy(g => g.Cursor).Skip(5).Take(5);
            var cursor = Context.Games.OrderBy(g => g.Cursor).Skip(4).First().Cursor;

            var sut = CreateSut();
            var actual = await sut.GetAsync(
                pagination: new PaginationFilter(cursor, 5),
                cancellationToken: TestContext.Current.CancellationToken);

            Assert.Equivalent(page, actual, true);
        }

        [Fact]
        public async Task ShouldReturnGamesWithNullOrganiser_WhenOrganiserHasBeenHardDeleted()
        {
            var organiser = SeedDataFactory.Users.Create(201);
            var game = SeedDataFactory.Games.Create(201, organiser);
            await Context.Users.AddAsync(organiser, TestContext.Current.CancellationToken);
            await Context.Games.AddAsync(game, TestContext.Current.CancellationToken);
            await Context.SaveChangesAsync(TestContext.Current.CancellationToken);

            await Context.Users.IgnoreQueryFilters().Where(u => u.Id == organiser.Id)
                .ExecuteDeleteAsync(TestContext.Current.CancellationToken);
            Context.ChangeTracker.Clear();

            var sut = CreateSut();
            var actual = await sut.GetAsync(
                pagination: new PaginationFilter(null, 200),
                cancellationToken: TestContext.Current.CancellationToken);

            var result = actual.Single(g => g.Id == game.Id);
            Assert.Null(result.OrganiserId);
            Assert.Null(result.Organiser);
        }
    }
}