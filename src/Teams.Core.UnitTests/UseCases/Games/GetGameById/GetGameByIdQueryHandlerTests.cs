using Teams.Core.Exceptions;
using Teams.Core.UseCases.Games.GetGameById;
using Teams.Domain.Entities;

namespace Teams.Core.UnitTests.UseCases.Games.GetGameById;

public static class GetGameByIdQueryHandlerTests
{
    public class HandleAsync : UseCaseTestBase<GetGameByIdQuery>
    {
        private GetGameByIdQueryHandler CreateSut() => new(GamesRepository);

        [Fact]
        public async Task ShouldThrowNotFoundException_WhenGameDoesNotExist()
        {
            GamesRepository.GetByIdAsync("missing-game", Arg.Any<CancellationToken>()).Returns((Game?)null);
            var query = new GetGameByIdQuery("missing-game");
            var sut = CreateSut();

            var exception = await Assert.ThrowsAsync<NotFoundException>(
                () => sut.HandleAsync(query, TestContext.Current.CancellationToken));

            Assert.Equal(nameof(Game), exception.ResourceType);
            Assert.Equal("missing-game", exception.ResourceIdentifier);
        }

        [Fact]
        public async Task ShouldReturnGame_WhenGameExists()
        {
            var existingGame = new Game("organiser-id", "location", DateTime.UtcNow, 60, 5);
            GamesRepository.GetByIdAsync(existingGame.Id, Arg.Any<CancellationToken>()).Returns(existingGame);
            var query = new GetGameByIdQuery(existingGame.Id);
            var sut = CreateSut();

            var result = await sut.HandleAsync(query, TestContext.Current.CancellationToken);

            Assert.Same(existingGame, result);
        }
    }
}