using Teams.Data.Models;
using Teams.Data.Repositories.Players;
using Teams.Domain.Enums;

namespace Teams.Data.UnitTests.Repositories.Players;

public static class ReadOnlyPlayersRepositoryTests
{
    public class GetByIdAsync : RepositoryTestBase
    {
        private ReadOnlyPlayersRepository CreateSut() => new(Context);

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
            var id = Context.Players.First().Id;
            var expected = await Context.Players.FindAsync([id], TestContext.Current.CancellationToken);

            var sut = CreateSut();
            var actual = await sut.GetByIdAsync(id, TestContext.Current.CancellationToken);
            Assert.Equivalent(expected, actual, true);
        }
    }

    public class GetAsync : RepositoryTestBase
    {
        private ReadOnlyPlayersRepository CreateSut() => new(Context);

        [Fact]
        public async Task ShouldReturnFirstPage_WhenNoParametersProvided()
        {
            var firstPage = Context.Players.OrderBy(p => p.Cursor).Take(Constants.DefaultPageSize);
            var sut = CreateSut();
            var actual = await sut.GetAsync(cancellationToken: TestContext.Current.CancellationToken);
            Assert.Equivalent(firstPage, actual, true);
        }

        [Fact]
        public async Task ShouldReturnFilteredPage_WhenGameIdProvided()
        {
            var value = Context.Games.First().Id;
            var expected = Context.Players.Where(p => p.GameId == value)
                .OrderBy(p => p.Cursor)
                .Take(Constants.DefaultPageSize);

            var sut = CreateSut();
            var actual = await sut.GetAsync(gameId: value, cancellationToken: TestContext.Current.CancellationToken);

            Assert.Equivalent(expected, actual, true);
        }

        [Fact]
        public async Task ShouldReturnFilteredPage_WhenDisplayNameProvided()
        {
            // Every game's first home-team dummy is seeded at index 100, so this matches one
            // player per game across the whole set.
            const string value = "00000100";
            var expected = Context.Players
                .Where(p => (p.User != null && p.User.DisplayName.Contains(value)) ||
                            (p.DisplayName != null && p.DisplayName.Contains(value)))
                .OrderBy(p => p.Cursor)
                .Take(Constants.DefaultPageSize);

            var sut = CreateSut();
            var actual = await sut.GetAsync(displayName: value, cancellationToken: TestContext.Current.CancellationToken);

            Assert.Equivalent(expected, actual, true);
        }

        [Fact]
        public async Task ShouldReturnFilteredPage_WhenUserIdProvided()
        {
            var value = GetUser(1).Id;
            var expected = Context.Players.Where(p => p.UserId == value)
                .OrderBy(p => p.Cursor)
                .Take(Constants.DefaultPageSize);

            var sut = CreateSut();
            var actual = await sut.GetAsync(userId: value, cancellationToken: TestContext.Current.CancellationToken);

            Assert.Equivalent(expected, actual, true);
        }

        [Fact]
        public async Task ShouldReturnFilteredPage_WhenRatingProvided()
        {
            var ordered = Context.Players.OrderBy(p => p.Rating).ToList();
            var from = ordered[9].Rating;
            var to = ordered[19].Rating;
            var expected = Context.Players.Where(p => p.Rating >= from && p.Rating < to)
                .OrderBy(p => p.Cursor)
                .Take(Constants.DefaultPageSize);

            var sut = CreateSut();
            var actual = await sut.GetAsync(
                rating: new RangeFilter<int>(from, to),
                cancellationToken: TestContext.Current.CancellationToken);

            Assert.Equivalent(expected, actual, true);
        }

        [Fact]
        public async Task ShouldReturnFilteredPage_WhenTeamProvided()
        {
            const GameTeamEnum value = GameTeamEnum.Away;
            var expected = Context.Players.Where(p => p.Team == value)
                .OrderBy(p => p.Cursor)
                .Take(Constants.DefaultPageSize);

            var sut = CreateSut();
            var actual = await sut.GetAsync(team: value, cancellationToken: TestContext.Current.CancellationToken);

            Assert.Equivalent(expected, actual, true);
        }

        [Fact]
        public async Task ShouldReturnFilteredPage_WhenTypeProvided()
        {
            const PlayerTypeEnum value = PlayerTypeEnum.User;
            var expected = Context.Players.Where(p => p.Type == value)
                .OrderBy(p => p.Cursor)
                .Take(Constants.DefaultPageSize);

            var sut = CreateSut();
            var actual = await sut.GetAsync(type: value, cancellationToken: TestContext.Current.CancellationToken);

            Assert.Equivalent(expected, actual, true);
        }

        [Fact]
        public async Task ShouldReturnFilteredPage_WhenCreatedFromProvided()
        {
            var value = Context.Players.OrderBy(p => p.DateCreated).Skip(29).First().DateCreated;
            var expected = Context.Players.Where(p => p.DateCreated >= value)
                .OrderBy(p => p.Cursor)
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
            var value = Context.Players.OrderBy(p => p.DateCreated).Skip(29).First().DateCreated;
            var expected = Context.Players.Where(p => p.DateCreated < value)
                .OrderBy(p => p.Cursor)
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
            var value = Context.Players.OrderBy(p => p.DateModified).Skip(29).First().DateModified;
            var expected = Context.Players.Where(p => p.DateModified >= value)
                .OrderBy(p => p.Cursor)
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
            var value = Context.Players.OrderBy(p => p.DateModified).Skip(29).First().DateModified;
            var expected = Context.Players.Where(p => p.DateModified < value)
                .OrderBy(p => p.Cursor)
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
            var page = Context.Players.OrderBy(p => p.Cursor).Skip(5).Take(5);
            var cursor = Context.Players.OrderBy(p => p.Cursor).Skip(4).First().Cursor;

            var sut = CreateSut();
            var actual = await sut.GetAsync(
                pagination: new PaginationFilter(cursor, 5),
                cancellationToken: TestContext.Current.CancellationToken);

            Assert.Equivalent(page, actual, true);
        }
    }
}