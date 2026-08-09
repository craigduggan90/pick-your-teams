using Teams.Core.UseCases.Players.GetPlayers;
using Teams.Data.Models;
using Teams.Domain.Entities;
using Teams.Domain.Enums;

namespace Teams.Core.UnitTests.UseCases.Players.GetPlayers;

public static class GetPlayersQueryHandlerTests
{
    public class HandleAsync : UseCaseTestBase<GetPlayersQuery>
    {
        private GetPlayersQueryHandler CreateSut() => new(PlayersRepository);

        [Fact]
        public async Task ShouldForwardAllFilters_ToRepository()
        {
            var createdFrom = new DateTime(2026, 1, 1);
            var createdTo = new DateTime(2026, 2, 1);
            var modifiedFrom = new DateTime(2026, 3, 1);
            var modifiedTo = new DateTime(2026, 4, 1);
            var query = new GetPlayersQuery(
                GameId: "game-id",
                DisplayName: "display-name",
                UserId: "user-id",
                RatingFrom: 900,
                RatingTo: 1100,
                Team: GameTeamEnum.Home,
                Type: PlayerTypeEnum.User,
                CreatedFrom: createdFrom,
                CreatedTo: createdTo,
                ModifiedFrom: modifiedFrom,
                ModifiedTo: modifiedTo,
                PageSize: 10,
                Cursor: 42);
            var sut = CreateSut();

            await sut.HandleAsync(query, TestContext.Current.CancellationToken);

            await PlayersRepository.Received(1).GetAsync(
                "game-id",
                "display-name",
                "user-id",
                new RangeFilter<int>(900, 1100),
                GameTeamEnum.Home,
                PlayerTypeEnum.User,
                new DateFilter(
                    new RangeFilter<DateTime>(createdFrom, createdTo),
                    new RangeFilter<DateTime>(modifiedFrom, modifiedTo)),
                new PaginationFilter(42, 10),
                Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task ShouldReturnEntities_AsReadOnlyCollection()
        {
            Player[] entities = [
                new("game-id", null,  1000, PlayerTypeEnum.Dummy, GameTeamEnum.None) { DisplayName = "display-one" },
                new("game-id", null, 1000, PlayerTypeEnum.Dummy, GameTeamEnum.None) { DisplayName = "display-two" }
            ];
            PlayersRepository.GetAsync(
                Arg.Any<string?>(),
                Arg.Any<string?>(),
                Arg.Any<string?>(),
                Arg.Any<RangeFilter<int>?>(),
                Arg.Any<GameTeamEnum?>(),
                Arg.Any<PlayerTypeEnum?>(),
                Arg.Any<DateFilter?>(),
                Arg.Any<PaginationFilter?>(),
                Arg.Any<CancellationToken>()).Returns(entities);
            var query = new GetPlayersQuery();
            var sut = CreateSut();

            var result = await sut.HandleAsync(query, TestContext.Current.CancellationToken);

            Assert.Equal(entities, result);
        }

        [Fact]
        public async Task ShouldReturnEmptyCollection_WhenNoEntitiesFound()
        {
            PlayersRepository.GetAsync(
                Arg.Any<string?>(),
                Arg.Any<string?>(),
                Arg.Any<string?>(),
                Arg.Any<RangeFilter<int>?>(),
                Arg.Any<GameTeamEnum?>(),
                Arg.Any<PlayerTypeEnum?>(),
                Arg.Any<DateFilter?>(),
                Arg.Any<PaginationFilter?>(),
                Arg.Any<CancellationToken>()).Returns([]);
            var query = new GetPlayersQuery();
            var sut = CreateSut();

            var result = await sut.HandleAsync(query, TestContext.Current.CancellationToken);

            Assert.Empty(result);
        }
    }
}