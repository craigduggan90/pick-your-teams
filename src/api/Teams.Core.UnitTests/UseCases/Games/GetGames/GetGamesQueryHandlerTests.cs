using Teams.Core.Models;
using Teams.Core.UseCases.Games.GetGames;
using Teams.Data.Models;
using Teams.Domain.Entities;
using Teams.Domain.Enums;

namespace Teams.Core.UnitTests.UseCases.Games.GetGames;

public static class GetGamesQueryHandlerTests
{
    public class HandleAsync : UseCaseTestBase<GetGamesQuery>
    {
        private GetGamesQueryHandler CreateSut() => new(GamesRepository, ActorAccessor);

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
                Ownership: null,
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
                organiserId: null,
                userId: null,
                organiserOrPlayerId: null,
                dateFilter: new DateFilter(
                    new RangeFilter<DateTime>(createdFrom, createdTo),
                    new RangeFilter<DateTime>(modifiedFrom, modifiedTo)),
                pagination: new PaginationFilter(42, 10),
                cancellationToken: Arg.Any<CancellationToken>());
        }

        [Theory]
        [InlineData(GameOwnershipEnum.Organising)]
        [InlineData(GameOwnershipEnum.Playing)]
        [InlineData(GameOwnershipEnum.Both)]
        public async Task ShouldResolveOwnership_AgainstTheActorsOwnId_NeverAClientSuppliedOne(GameOwnershipEnum ownership)
        {
            ActorAccessor.Current.Returns(new Actor("actor-id", "actor-tag", "actor-display-name"));
            var query = CreateQuery(ownership);
            var sut = CreateSut();

            await sut.HandleAsync(query, TestContext.Current.CancellationToken);

            var expectedOrganiserId = ownership == GameOwnershipEnum.Organising ? "actor-id" : null;
            var expectedUserId = ownership == GameOwnershipEnum.Playing ? "actor-id" : null;
            var expectedOrganiserOrPlayerId = ownership == GameOwnershipEnum.Both ? "actor-id" : null;

            await GamesRepository.Received(1).GetAsync(
                location: Arg.Any<string?>(),
                startTime: Arg.Any<RangeFilter<DateTime>?>(),
                duration: Arg.Any<RangeFilter<int>?>(),
                teamSize: Arg.Any<int?>(),
                status: Arg.Any<GameStatusEnum?>(),
                organiserId: expectedOrganiserId,
                userId: expectedUserId,
                organiserOrPlayerId: expectedOrganiserOrPlayerId,
                dateFilter: Arg.Any<DateFilter?>(),
                pagination: Arg.Any<PaginationFilter?>(),
                cancellationToken: Arg.Any<CancellationToken>());
        }

        private static GetGamesQuery CreateQuery(GameOwnershipEnum? ownership) => new(
            Location: null,
            StartTimeFrom: null,
            StartTimeTo: null,
            DurationFrom: null,
            DurationTo: null,
            TeamSize: null,
            Status: null,
            Ownership: ownership,
            CreatedFrom: null,
            CreatedTo: null,
            ModifiedFrom: null,
            ModifiedTo: null,
            PageSize: null,
            Cursor: null);

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
                organiserId: Arg.Any<string?>(),
                userId: Arg.Any<string?>(),
                organiserOrPlayerId: Arg.Any<string?>(),
                dateFilter: Arg.Any<DateFilter?>(),
                pagination: Arg.Any<PaginationFilter?>(),
                cancellationToken: Arg.Any<CancellationToken>()).Returns(entities);
            var query = CreateQuery(ownership: null);
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
                organiserId: Arg.Any<string?>(),
                userId: Arg.Any<string?>(),
                organiserOrPlayerId: Arg.Any<string?>(),
                dateFilter: Arg.Any<DateFilter?>(),
                pagination: Arg.Any<PaginationFilter?>(),
                cancellationToken: Arg.Any<CancellationToken>()).Returns([]);
            var query = CreateQuery(ownership: null);
            var sut = CreateSut();

            var result = await sut.HandleAsync(query, TestContext.Current.CancellationToken);

            Assert.Empty(result);
        }
    }
}