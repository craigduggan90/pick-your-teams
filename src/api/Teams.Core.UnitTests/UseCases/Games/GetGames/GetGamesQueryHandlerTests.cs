using Teams.Core.UseCases.Games.GetGames;
using Teams.Data.Models;
using Teams.Domain.Entities;
using Teams.Domain.Enums;

namespace Teams.Core.UnitTests.UseCases.Games.GetGames;

public static class GetGamesQueryHandlerTests
{
    public class HandleAsync : UseCaseTestBase<GetGamesQuery>
    {
        private GetGamesQueryHandler CreateSut() => new(GamesRepository);

        [Fact]
        public async Task ShouldForwardAllFilters_ToRepository()
        {
            var startTimeFrom = new DateTime(2026, 1, 1);
            var startTimeTo = new DateTime(2026, 2, 1);
            var createdFrom = new DateTime(2026, 3, 1);
            var createdTo = new DateTime(2026, 4, 1);
            var modifiedFrom = new DateTime(2026, 5, 1);
            var modifiedTo = new DateTime(2026, 6, 1);
            const GameStatusEnum status = GameStatusEnum.Scheduled;
            var query = new GetGamesQuery(
                Location: "location",
                StartTimeFrom: startTimeFrom,
                StartTimeTo: startTimeTo,
                DurationFrom: 30,
                DurationTo: 90,
                TeamSize: 5,
                Status: status,
                CreatedFrom: createdFrom,
                CreatedTo: createdTo,
                ModifiedFrom: modifiedFrom,
                ModifiedTo: modifiedTo,
                PageSize: 10,
                Cursor: 42);
            var sut = CreateSut();

            await sut.HandleAsync(query, TestContext.Current.CancellationToken);

            await GamesRepository.Received(1).GetAsync(
                location: "location",
                startTime: new RangeFilter<DateTime>(startTimeFrom, startTimeTo),
                duration: new RangeFilter<int>(30, 90),
                teamSize: 5,
                status: status,
                dateFilter: new DateFilter(
                    new RangeFilter<DateTime>(createdFrom, createdTo),
                    new RangeFilter<DateTime>(modifiedFrom, modifiedTo)),
                pagination: new PaginationFilter(42, 10),
                cancellationToken: Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task ShouldReturnEntities_AsReadOnlyCollection()
        {
            Game[] entities = [
                new("organiser-id", "location-one", DateTime.UtcNow, 60, 5),
                new("organiser-id", "location-two", DateTime.UtcNow, 60, 5)
            ];
            GamesRepository.GetAsync(
                location: Arg.Any<string?>(),
                startTime: Arg.Any<RangeFilter<DateTime>?>(),
                duration: Arg.Any<RangeFilter<int>?>(),
                teamSize: Arg.Any<int?>(),
                status: Arg.Any<GameStatusEnum?>(),
                dateFilter: Arg.Any<DateFilter?>(),
                pagination: Arg.Any<PaginationFilter?>(),
                cancellationToken: Arg.Any<CancellationToken>()).Returns(entities);
            var query = new GetGamesQuery(
                Location: null,
                StartTimeFrom: null,
                StartTimeTo: null,
                DurationFrom: null,
                DurationTo: null,
                TeamSize: null,
                Status: null,
                CreatedFrom: null,
                CreatedTo: null,
                ModifiedFrom: null,
                ModifiedTo: null,
                PageSize: null,
                Cursor: null);
            var sut = CreateSut();

            var result = await sut.HandleAsync(query, TestContext.Current.CancellationToken);

            Assert.Equal(entities, result);
        }

        [Fact]
        public async Task ShouldReturnEmptyCollection_WhenNoEntitiesFound()
        {
            GamesRepository.GetAsync(
                location: Arg.Any<string?>(),
                startTime: Arg.Any<RangeFilter<DateTime>?>(),
                duration: Arg.Any<RangeFilter<int>?>(),
                teamSize: Arg.Any<int?>(),
                status: Arg.Any<GameStatusEnum?>(),
                dateFilter: Arg.Any<DateFilter?>(),
                pagination: Arg.Any<PaginationFilter?>(),
                cancellationToken: Arg.Any<CancellationToken>()).Returns([]);
            var query = new GetGamesQuery(
                Location: null,
                StartTimeFrom: null,
                StartTimeTo: null,
                DurationFrom: null,
                DurationTo: null,
                TeamSize: null,
                Status: null,
                CreatedFrom: null,
                CreatedTo: null,
                ModifiedFrom: null,
                ModifiedTo: null,
                PageSize: null,
                Cursor: null);
            var sut = CreateSut();

            var result = await sut.HandleAsync(query, TestContext.Current.CancellationToken);

            Assert.Empty(result);
        }
    }
}