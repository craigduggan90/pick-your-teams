using Teams.Data.Models;
using Teams.Data.Repositories.Invitations;
using Teams.Domain.Enums;

namespace Teams.Data.UnitTests.Repositories.Invitations;

public static class ReadOnlyInvitationsRepositoryTests
{
    public class GetByIdAsync : RepositoryTestBase
    {
        private ReadOnlyInvitationsRepository CreateSut() => new(Context);

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
            var id = SeedDataFactory.Invitations.GetIdentifier(1);
            var expected = await Context.Invitations.FindAsync([id], TestContext.Current.CancellationToken);

            var sut = CreateSut();
            var actual = await sut.GetByIdAsync(id, TestContext.Current.CancellationToken);
            Assert.Equivalent(expected, actual, true);
        }
    }

    public class GetInvitationsAsync : RepositoryTestBase
    {
        private ReadOnlyInvitationsRepository CreateSut() => new(Context);

        [Fact]
        public async Task ShouldReturnFirstPage_WhenNoParametersProvided()
        {
            var firstPage = Context.Invitations.OrderBy(i => i.Cursor).Take(Constants.DefaultPageSize);
            var sut = CreateSut();
            var actual = await sut.GetInvitationsAsync(cancellationToken: TestContext.Current.CancellationToken);
            Assert.Equivalent(firstPage, actual, true);
        }

        [Fact]
        public async Task ShouldReturnFilteredPage_WhenGameIdProvided()
        {
            var value = Context.Games.Skip(20).First().Id;
            var expected = Context.Invitations.Where(i => i.GameId == value)
                .OrderBy(i => i.Cursor)
                .Take(Constants.DefaultPageSize);

            var sut = CreateSut();
            var actual = await sut.GetInvitationsAsync(gameId: value, cancellationToken: TestContext.Current.CancellationToken);

            Assert.Equivalent(expected, actual, true);
        }

        [Fact]
        public async Task ShouldReturnFilteredPage_WhenUserIdProvided()
        {
            var value = Context.Invitations.Where(i => i.UserId != null).Skip(2).First().UserId;
            var expected = Context.Invitations.Where(i => i.UserId == value)
                .OrderBy(i => i.Cursor)
                .Take(Constants.DefaultPageSize);

            var sut = CreateSut();
            var actual = await sut.GetInvitationsAsync(userId: value, cancellationToken: TestContext.Current.CancellationToken);

            Assert.Equivalent(expected, actual, true);
        }

        [Fact]
        public async Task ShouldReturnFilteredPage_WhenEmailAddressProvided()
        {
            var value = Context.Invitations.Skip(20).First().EmailAddress;
            var expected = Context.Invitations.Where(i => i.EmailAddress.Contains(value))
                .OrderBy(i => i.Cursor)
                .Take(Constants.DefaultPageSize);

            var sut = CreateSut();
            var actual = await sut.GetInvitationsAsync(emailAddress: value, cancellationToken: TestContext.Current.CancellationToken);

            Assert.Equivalent(expected, actual, true);
        }

        [Fact]
        public async Task ShouldReturnFilteredPage_WhenStatusProvided()
        {
            const InvitationStatusEnum value = InvitationStatusEnum.Accepted;
            var expected = Context.Invitations.Where(i => i.Status == value)
                .OrderBy(i => i.Cursor)
                .Take(Constants.DefaultPageSize);

            var sut = CreateSut();
            var actual = await sut.GetInvitationsAsync(status: value, cancellationToken: TestContext.Current.CancellationToken);

            Assert.Equivalent(expected, actual, true);
        }

        [Fact]
        public async Task ShouldReturnFilteredPage_WhenCreatedFromProvided()
        {
            var value = Context.Invitations.OrderBy(i => i.DateCreated).Skip(89).First().DateCreated;
            var expected = Context.Invitations.Where(i => i.DateCreated >= value)
                .OrderBy(i => i.Cursor)
                .Take(Constants.DefaultPageSize);

            var sut = CreateSut();
            var actual = await sut.GetInvitationsAsync(
                dateFilter: new DateFilter(new RangeFilter<DateTime>(value, null), null),
                cancellationToken: TestContext.Current.CancellationToken);

            Assert.Equivalent(expected, actual, true);
        }

        [Fact]
        public async Task ShouldReturnFilteredPage_WhenCreatedToProvided()
        {
            var value = Context.Invitations.OrderBy(i => i.DateCreated).Skip(19).First().DateCreated;
            var expected = Context.Invitations.Where(i => i.DateCreated < value)
                .OrderBy(i => i.Cursor)
                .Take(Constants.DefaultPageSize);

            var sut = CreateSut();
            var actual = await sut.GetInvitationsAsync(
                dateFilter: new DateFilter(new RangeFilter<DateTime>(null, value), null),
                cancellationToken: TestContext.Current.CancellationToken);

            Assert.Equivalent(expected, actual, true);
        }

        [Fact]
        public async Task ShouldReturnFilteredPage_WhenModifiedFromProvided()
        {
            var value = Context.Invitations.OrderBy(i => i.DateModified).Skip(89).First().DateModified;
            var expected = Context.Invitations.Where(i => i.DateModified >= value)
                .OrderBy(i => i.Cursor)
                .Take(Constants.DefaultPageSize);

            var sut = CreateSut();
            var actual = await sut.GetInvitationsAsync(
                dateFilter: new DateFilter(null, new RangeFilter<DateTime>(value, null)),
                cancellationToken: TestContext.Current.CancellationToken);

            Assert.Equivalent(expected, actual, true);
        }

        [Fact]
        public async Task ShouldReturnFilteredPage_WhenModifiedToProvided()
        {
            var value = Context.Invitations.OrderBy(i => i.DateModified).Skip(19).First().DateModified;
            var expected = Context.Invitations.Where(i => i.DateModified < value)
                .OrderBy(i => i.Cursor)
                .Take(Constants.DefaultPageSize);

            var sut = CreateSut();
            var actual = await sut.GetInvitationsAsync(
                dateFilter: new DateFilter(null, new RangeFilter<DateTime>(null, value)),
                cancellationToken: TestContext.Current.CancellationToken);

            Assert.Equivalent(expected, actual, true);
        }

        [Fact]
        public async Task ShouldReturnSpecifiedPage_WhenPaginationFilterProvided()
        {
            var page = Context.Invitations.OrderBy(i => i.Cursor).Skip(5).Take(5);
            var cursor = Context.Invitations.OrderBy(i => i.Cursor).Skip(4).First().Cursor;

            var sut = CreateSut();
            var actual = await sut.GetInvitationsAsync(
                pagination: new PaginationFilter(cursor, 5),
                cancellationToken: TestContext.Current.CancellationToken);

            Assert.Equivalent(page, actual, true);
        }
    }
}