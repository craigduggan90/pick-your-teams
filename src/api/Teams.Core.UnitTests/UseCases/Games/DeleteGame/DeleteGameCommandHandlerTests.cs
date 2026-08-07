using Teams.Core.Exceptions;
using Teams.Core.Models;
using Teams.Core.UseCases.Games.DeleteGame;
using Teams.Domain.Entities;

namespace Teams.Core.UnitTests.UseCases.Games.DeleteGame;

public static class DeleteGameCommandHandlerTests
{
    public class HandleAsync : UseCaseTestBase<DeleteGameCommand>
    {
        private static Game CreateExistingGame() =>
            new("organiser-id", "existing-location", DateTime.UtcNow, 60, 5);

        private DeleteGameCommandHandler CreateSut() =>
            new(UnitOfWork, ActorAccessor, new FakeLogger<DeleteGameCommandHandler>());

        [Fact]
        public async Task ShouldThrowNotFoundException_WhenGameDoesNotExist()
        {
            GamesRepository.GetByIdAsync("missing-game", Arg.Any<CancellationToken>()).Returns((Game?)null);
            var command = new DeleteGameCommand("missing-game");
            var sut = CreateSut();

            var exception = await Assert.ThrowsAsync<NotFoundException>(
                () => sut.HandleAsync(command, TestContext.Current.CancellationToken));

            Assert.Equal(nameof(Game), exception.ResourceType);
            Assert.Equal("missing-game", exception.ResourceIdentifier);
        }

        [Fact]
        public async Task ShouldThrowAccessDeniedExceptionAndNotPersistChanges_WhenActorIsNotOrganiser()
        {
            var existingGame = CreateExistingGame();
            GamesRepository.GetByIdAsync(existingGame.Id, Arg.Any<CancellationToken>()).Returns(existingGame);
            ActorAccessor.Current.Returns(new Actor("some-other-actor", "tag", "display-name"));
            var command = new DeleteGameCommand(existingGame.Id);
            var sut = CreateSut();

            await Assert.ThrowsAsync<AccessDeniedException>(
                () => sut.HandleAsync(command, TestContext.Current.CancellationToken));

            await GamesRepository.DidNotReceive().UpdateAsync(Arg.Any<Game>(), Arg.Any<CancellationToken>());
            await UnitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task ShouldPersistDeletionAndReturnTheGame_WhenGameExists()
        {
            var existingGame = CreateExistingGame();
            GamesRepository.GetByIdAsync(existingGame.Id, Arg.Any<CancellationToken>()).Returns(existingGame);
            var command = new DeleteGameCommand(existingGame.Id);
            var sut = CreateSut();

            var result = await sut.HandleAsync(command, TestContext.Current.CancellationToken);

            Assert.Same(existingGame, result);
            Assert.NotNull(result.DateDeleted);
            await GamesRepository.Received(1).UpdateAsync(existingGame, Arg.Any<CancellationToken>());
            await UnitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
        }
    }
}