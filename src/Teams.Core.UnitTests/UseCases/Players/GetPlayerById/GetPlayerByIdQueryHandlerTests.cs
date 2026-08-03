using Teams.Core.Exceptions;
using Teams.Core.UseCases.Players.GetPlayerById;
using Teams.Domain.Entities;

namespace Teams.Core.UnitTests.UseCases.Players.GetPlayerById;

public static class GetPlayerByIdQueryHandlerTests
{
    public class HandleAsync : UseCaseTestBase<GetPlayerByIdQuery>
    {
        private GetPlayerByIdQueryHandler CreateSut() => new(PlayersRepository);

        [Fact]
        public async Task ShouldThrowNotFoundException_WhenPlayerDoesNotExist()
        {
            PlayersRepository.GetByIdAsync("missing-player", Arg.Any<CancellationToken>()).Returns((Player?)null);
            var query = new GetPlayerByIdQuery("missing-player");
            var sut = CreateSut();

            var exception = await Assert.ThrowsAsync<NotFoundException>(
                () => sut.HandleAsync(query, TestContext.Current.CancellationToken));

            Assert.Equal(nameof(Player), exception.ResourceType);
            Assert.Equal("missing-player", exception.ResourceIdentifier);
        }

        [Fact]
        public async Task ShouldReturnPlayer_WhenPlayerExists()
        {
            var existingPlayer = new Player("game-id", null, "display-name", 1000,
                Teams.Domain.Enums.PlayerTypeEnum.Dummy, Teams.Domain.Enums.GameTeamEnum.None);
            PlayersRepository.GetByIdAsync(existingPlayer.Id, Arg.Any<CancellationToken>()).Returns(existingPlayer);
            var query = new GetPlayerByIdQuery(existingPlayer.Id);
            var sut = CreateSut();

            var result = await sut.HandleAsync(query, TestContext.Current.CancellationToken);

            Assert.Same(existingPlayer, result);
        }
    }
}