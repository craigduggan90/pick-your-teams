using Teams.Core.UseCases.Users.GetUsers;
using Teams.Data.Models;
using Teams.Domain.Entities;

namespace Teams.Core.UnitTests.UseCases.Users.GetUsers;

public static class GetUsersQueryHandlerTests
{
    public class HandleAsync : UseCaseTestBase<GetUsersQuery>
    {
        private GetUsersQueryHandler CreateSut() => new(UsersRepository);

        [Fact]
        public async Task ShouldForwardAllFilters_ToRepository()
        {
            var createdFrom = new DateTime(2026, 1, 1);
            var createdTo = new DateTime(2026, 2, 1);
            var modifiedFrom = new DateTime(2026, 3, 1);
            var modifiedTo = new DateTime(2026, 4, 1);
            var query = new GetUsersQuery(
                EmailAddress: "user@example.com",
                Tag: "tag-value",
                DisplayName: "display-name",
                RatingFrom: 900,
                RatingTo: 1100,
                CreatedFrom: createdFrom,
                CreatedTo: createdTo,
                ModifiedFrom: modifiedFrom,
                ModifiedTo: modifiedTo,
                PageSize: 10,
                Cursor: 42);
            var sut = CreateSut();

            await sut.HandleAsync(query, TestContext.Current.CancellationToken);

            await UsersRepository.Received(1).GetAsync(
                emailAddress: "user@example.com",
                tag: "tag-value",
                displayName: "display-name",
                rating: new RangeFilter<int>(900, 1100),
                dateFilter: new DateFilter(
                    new RangeFilter<DateTime>(createdFrom, createdTo),
                    new RangeFilter<DateTime>(modifiedFrom, modifiedTo)),
                pagination: new PaginationFilter(42, 10),
                cancellationToken: Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task ShouldReturnEntities_AsReadOnlyCollection()
        {
            User[] entities = [
                new("display-one", "external-one", "one@example.com", null),
                new("display-two", "external-two", "two@example.com", null)
            ];
            UsersRepository.GetAsync(
                emailAddress: Arg.Any<string?>(),
                tag: Arg.Any<string?>(),
                displayName: Arg.Any<string?>(),
                rating: Arg.Any<RangeFilter<int>?>(),
                dateFilter: Arg.Any<DateFilter?>(),
                pagination: Arg.Any<PaginationFilter?>(),
                cancellationToken: Arg.Any<CancellationToken>()).Returns(entities);
            var query = new GetUsersQuery();
            var sut = CreateSut();

            var result = await sut.HandleAsync(query, TestContext.Current.CancellationToken);

            Assert.Equal(entities, result);
        }

        [Fact]
        public async Task ShouldReturnEmptyCollection_WhenNoEntitiesFound()
        {
            UsersRepository.GetAsync(
                emailAddress: Arg.Any<string?>(),
                tag: Arg.Any<string?>(),
                displayName: Arg.Any<string?>(),
                rating: Arg.Any<RangeFilter<int>?>(),
                dateFilter: Arg.Any<DateFilter?>(),
                pagination: Arg.Any<PaginationFilter?>(),
                cancellationToken: Arg.Any<CancellationToken>()).Returns([]);
            var query = new GetUsersQuery();
            var sut = CreateSut();

            var result = await sut.HandleAsync(query, TestContext.Current.CancellationToken);

            Assert.Empty(result);
        }
    }
}