using Teams.Data.Models;
using Teams.Data.Repositories.Users;

namespace Teams.Data.UnitTests.Repositories.Users;

public static class ReadOnlyUsersRepositoryTests
{
    public class GetByIdAsync : RepositoryTestBase
    {
        private ReadOnlyUsersRepository CreateSut() => new(Context);

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
            var id = SeedDataFactory.Users.GetIdentifier(1);
            var expected = await Context.Users.FindAsync([id], TestContext.Current.CancellationToken);

            var sut = CreateSut();
            var actual = await sut.GetByIdAsync(id, TestContext.Current.CancellationToken);
            Assert.Equivalent(expected, actual, true);
        }
    }

    public class GetByTagAsync : RepositoryTestBase
    {
        private ReadOnlyUsersRepository CreateSut() => new(Context);

        [Fact]
        public async Task ShouldReturnNull_WhenRecordDoesNotExist()
        {
            const string tag = "does-not-exist";
            var sut = CreateSut();
            var actual = await sut.GetByTagAsync(tag, TestContext.Current.CancellationToken);
            Assert.Null(actual);
        }

        [Fact]
        public async Task ShouldReturnEntity_WhenRecordExists()
        {
            const string tag = "tag-00000001";
            var expected = Context.Users.Single(u => u.Tag == tag);

            var sut = CreateSut();
            var actual = await sut.GetByTagAsync(tag, TestContext.Current.CancellationToken);
            Assert.Equivalent(expected, actual, true);
        }

        [Fact]
        public async Task ShouldReturnEntity_WhenRecordExistsWithDifferentCasing()
        {
            const string tag = "TAG-00000001";
            var expected = Context.Users.Single(u => u.Tag == "tag-00000001");

            var sut = CreateSut();
            var actual = await sut.GetByTagAsync(tag, TestContext.Current.CancellationToken);
            Assert.Equivalent(expected, actual, true);
        }
    }

    public class GetByExternalIdAsync : RepositoryTestBase
    {
        private ReadOnlyUsersRepository CreateSut() => new(Context);

        [Fact]
        public async Task ShouldReturnNull_WhenRecordDoesNotExist()
        {
            const string externalId = "does-not-exist";
            var sut = CreateSut();
            var actual = await sut.GetByExternalIdAsync(externalId, TestContext.Current.CancellationToken);
            Assert.Null(actual);
        }

        [Fact]
        public async Task ShouldReturnEntity_WhenRecordExists()
        {
            var externalId = $"test|{SeedDataFactory.Users.GetIdentifier(1)}";
            var expected = Context.Users.Single(u => u.ExternalId == externalId);

            var sut = CreateSut();
            var actual = await sut.GetByExternalIdAsync(externalId, TestContext.Current.CancellationToken);
            Assert.Equivalent(expected, actual, true);
        }
    }

    public class GetByEmailAddressAsync : RepositoryTestBase
    {
        private ReadOnlyUsersRepository CreateSut() => new(Context);

        [Fact]
        public async Task ShouldReturnNull_WhenRecordDoesNotExist()
        {
            const string emailAddress = "does-not-exist@test.io";
            var sut = CreateSut();
            var actual = await sut.GetByEmailAddressAsync(emailAddress, TestContext.Current.CancellationToken);
            Assert.Null(actual);
        }

        [Fact]
        public async Task ShouldReturnEntity_WhenRecordExists()
        {
            var emailAddress = $"{SeedDataFactory.Users.GetIdentifier(1)}@test.io";
            var expected = Context.Users.Single(u => u.EmailAddress == emailAddress);

            var sut = CreateSut();
            var actual = await sut.GetByEmailAddressAsync(emailAddress, TestContext.Current.CancellationToken);
            Assert.Equivalent(expected, actual, true);
        }

        [Fact]
        public async Task ShouldReturnEntity_WhenRecordExistsWithDifferentCasing()
        {
            var storedEmailAddress = $"{SeedDataFactory.Users.GetIdentifier(1)}@test.io";
            var emailAddress = storedEmailAddress.ToUpperInvariant();
            var expected = Context.Users.Single(u => u.EmailAddress == storedEmailAddress);

            var sut = CreateSut();
            var actual = await sut.GetByEmailAddressAsync(emailAddress, TestContext.Current.CancellationToken);
            Assert.Equivalent(expected, actual, true);
        }
    }

    public class GetByPhoneNumberAsync : RepositoryTestBase
    {
        private ReadOnlyUsersRepository CreateSut() => new(Context);

        [Fact]
        public async Task ShouldReturnNull_WhenRecordDoesNotExist()
        {
            const string mobile = "does-not-exist";
            var sut = CreateSut();
            var actual = await sut.GetByPhoneNumberAsync(mobile, TestContext.Current.CancellationToken);
            Assert.Null(actual);
        }

        [Fact]
        public async Task ShouldReturnEntity_WhenRecordExists()
        {
            // Odd index - even-indexed seeded users have a null Mobile.
            const int index = 1;
            var mobile = $"07{index:D9}";
            var expected = Context.Users.Single(u => u.Mobile == mobile);

            var sut = CreateSut();
            var actual = await sut.GetByPhoneNumberAsync(mobile, TestContext.Current.CancellationToken);
            Assert.Equivalent(expected, actual, true);
        }
    }

    public class GetAsync : RepositoryTestBase
    {
        private ReadOnlyUsersRepository CreateSut() => new(Context);

        [Fact]
        public async Task ShouldReturnFirstPage_WhenNoParametersProvided()
        {
            var firstPage = Context.Users.OrderBy(u => u.Cursor).Take(Constants.DefaultPageSize);
            var sut = CreateSut();
            var actual = await sut.GetAsync(null, null, null, null, cancellationToken: TestContext.Current.CancellationToken);

            Assert.Equal(Constants.DefaultPageSize, actual.Count());
            Assert.Equivalent(firstPage, actual, true);
        }

        [Fact]
        public async Task ShouldReturnFilteredPage_WhenEmailAddressProvided()
        {
            var value = SeedDataFactory.Users.GetIdentifier(1);
            var expected = Context.Users.Where(u => u.EmailAddress.Contains(value))
                .OrderBy(u => u.Cursor)
                .Take(Constants.DefaultPageSize);

            var sut = CreateSut();
            var actual = await sut.GetAsync(value, null, null, null, cancellationToken: TestContext.Current.CancellationToken);

            Assert.Equivalent(expected, actual, true);
        }

        [Fact]
        public async Task ShouldReturnFilteredPage_WhenTagProvided()
        {
            const string value = "tag-00000001";
            var expected = Context.Users.Where(u => u.Tag.Contains(value))
                .OrderBy(u => u.Cursor)
                .Take(Constants.DefaultPageSize);

            var sut = CreateSut();
            var actual = await sut.GetAsync(null, value, null, null, cancellationToken: TestContext.Current.CancellationToken);

            Assert.Equivalent(expected, actual, true);
        }

        [Fact]
        public async Task ShouldReturnFilteredPage_WhenDisplayNameProvided()
        {
            const string value = "00000001";
            var expected = Context.Users.Where(u => u.DisplayName.Contains(value))
                .OrderBy(u => u.Cursor)
                .Take(Constants.DefaultPageSize);

            var sut = CreateSut();
            var actual = await sut.GetAsync(null, null, value, null, cancellationToken: TestContext.Current.CancellationToken);

            Assert.Equivalent(expected, actual, true);
        }

        [Fact]
        public async Task ShouldReturnFilteredPage_WhenRatingProvided()
        {
            var ordered = Context.Users.OrderBy(u => u.Rating).ToList();
            var from = ordered[9].Rating;
            var to = ordered[14].Rating;
            var expected = Context.Users.Where(u => u.Rating >= from && u.Rating < to)
                .OrderBy(u => u.Cursor)
                .Take(Constants.DefaultPageSize);

            var sut = CreateSut();
            var actual = await sut.GetAsync(
                null, null, null,
                new RangeFilter<int>(from, to),
                cancellationToken: TestContext.Current.CancellationToken);

            Assert.Equivalent(expected, actual, true);
        }

        [Fact]
        public async Task ShouldReturnFilteredPage_WhenCreatedFromProvided()
        {
            var value = Context.Users.OrderBy(u => u.DateCreated).Skip(9).First().DateCreated;
            var expected = Context.Users.Where(u => u.DateCreated >= value)
                .OrderBy(u => u.Cursor)
                .Take(Constants.DefaultPageSize);

            var sut = CreateSut();
            var actual = await sut.GetAsync(
                null, null, null, null,
                dateFilter: new DateFilter(new RangeFilter<DateTime>(value, null), null),
                cancellationToken: TestContext.Current.CancellationToken);

            Assert.Equivalent(expected, actual, true);
        }

        [Fact]
        public async Task ShouldReturnFilteredPage_WhenCreatedToProvided()
        {
            var value = Context.Users.OrderBy(u => u.DateCreated).Skip(9).First().DateCreated;
            var expected = Context.Users.Where(u => u.DateCreated < value)
                .OrderBy(u => u.Cursor)
                .Take(Constants.DefaultPageSize);

            var sut = CreateSut();
            var actual = await sut.GetAsync(
                null, null, null, null,
                dateFilter: new DateFilter(new RangeFilter<DateTime>(null, value), null),
                cancellationToken: TestContext.Current.CancellationToken);

            Assert.Equivalent(expected, actual, true);
        }

        [Fact]
        public async Task ShouldReturnFilteredPage_WhenModifiedFromProvided()
        {
            var value = Context.Users.OrderBy(u => u.DateModified).Skip(9).First().DateModified;
            var expected = Context.Users.Where(u => u.DateModified >= value)
                .OrderBy(u => u.Cursor)
                .Take(Constants.DefaultPageSize);

            var sut = CreateSut();
            var actual = await sut.GetAsync(
                null, null, null, null,
                dateFilter: new DateFilter(null, new RangeFilter<DateTime>(value, null)),
                cancellationToken: TestContext.Current.CancellationToken);

            Assert.Equivalent(expected, actual, true);
        }

        [Fact]
        public async Task ShouldReturnFilteredPage_WhenModifiedToProvided()
        {
            var value = Context.Users.OrderBy(u => u.DateModified).Skip(9).First().DateModified;
            var expected = Context.Users.Where(u => u.DateModified < value)
                .OrderBy(u => u.Cursor)
                .Take(Constants.DefaultPageSize);

            var sut = CreateSut();
            var actual = await sut.GetAsync(
                null, null, null, null,
                dateFilter: new DateFilter(null, new RangeFilter<DateTime>(null, value)),
                cancellationToken: TestContext.Current.CancellationToken);

            Assert.Equivalent(expected, actual, true);
        }

        [Fact]
        public async Task ShouldReturnSpecifiedPage_WhenPaginationFilterProvided()
        {
            var page = Context.Users.OrderBy(u => u.Cursor).Skip(5).Take(5);
            var cursor = Context.Users.OrderBy(u => u.Cursor).Skip(4).First().Cursor;

            var sut = CreateSut();
            var actual = await sut.GetAsync(
                null, null, null, null,
                pagination: new PaginationFilter(cursor, 5),
                cancellationToken: TestContext.Current.CancellationToken);

            Assert.Equivalent(page, actual, true);
        }
    }
}