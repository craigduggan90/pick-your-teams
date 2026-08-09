using Teams.Core.Exceptions;
using Teams.Core.Models;
using Teams.Core.UseCases.Games.UpdateGame;
using Teams.Domain.Entities;
using Teams.Domain.Enums;

namespace Teams.Core.UnitTests.UseCases.Games.UpdateGame;

public static class UpdateGameCommandHandlerTests
{
    public class HandleAsync : UseCaseTestBase<UpdateGameCommand>
    {
        private static Game CreateExistingGame() =>
            new("organiser-id", "existing-location", DateTime.UtcNow, 60, 5);

        private UpdateGameCommandHandler CreateSut() =>
            new(UnitOfWork, ActorAccessor, Validator, new FakeLogger<UpdateGameCommandHandler>());

        [Fact]
        public async Task ShouldThrowCommandValidationException_WhenValidationFails()
        {
            SetupValidator(InvalidResult());
            var command = new UpdateGameCommand("game-id", null, null, null);
            var sut = CreateSut();

            await Assert.ThrowsAsync<CommandValidationException>(
                () => sut.HandleAsync(command, TestContext.Current.CancellationToken));
        }

        [Fact]
        public async Task ShouldNotLoadGame_WhenValidationFails()
        {
            SetupValidator(InvalidResult());
            var command = new UpdateGameCommand("game-id", null, null, null);
            var sut = CreateSut();

            await Assert.ThrowsAsync<CommandValidationException>(
                () => sut.HandleAsync(command, TestContext.Current.CancellationToken));

            await GamesRepository.DidNotReceive().GetByIdAsync(Arg.Any<string>(), Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task ShouldThrowNotFoundException_WhenGameDoesNotExist()
        {
            GamesRepository.GetByIdAsync("missing-game", Arg.Any<CancellationToken>()).Returns((Game?)null);
            var command = new UpdateGameCommand("missing-game", null, null, null);
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
            var command = new UpdateGameCommand(existingGame.Id, "new-location", null, null);
            var sut = CreateSut();

            await Assert.ThrowsAsync<AccessDeniedException>(
                () => sut.HandleAsync(command, TestContext.Current.CancellationToken));

            await GamesRepository.DidNotReceive().UpdateAsync(Arg.Any<Game>(), Arg.Any<CancellationToken>());
            await UnitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task ShouldThrowRequestHandlerExceptionAndNotPersistChanges_WhenGameIsFinished()
        {
            var existingGame = CreateExistingGame();
            existingGame.SetResult(GameTeamEnum.Home);
            GamesRepository.GetByIdAsync(existingGame.Id, Arg.Any<CancellationToken>()).Returns(existingGame);
            var command = new UpdateGameCommand(existingGame.Id, "new-location", null, null);
            var sut = CreateSut();

            await Assert.ThrowsAsync<RequestHandlerException>(
                () => sut.HandleAsync(command, TestContext.Current.CancellationToken));

            await GamesRepository.DidNotReceive().UpdateAsync(Arg.Any<Game>(), Arg.Any<CancellationToken>());
            await UnitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task ShouldNotPersistChanges_WhenUpdateResultsInNoActualChange()
        {
            var existingGame = CreateExistingGame();
            GamesRepository.GetByIdAsync(existingGame.Id, Arg.Any<CancellationToken>()).Returns(existingGame);
            var command = new UpdateGameCommand(
                existingGame.Id, existingGame.Location, existingGame.StartTime, existingGame.Duration);
            var sut = CreateSut();

            await sut.HandleAsync(command, TestContext.Current.CancellationToken);

            await GamesRepository.DidNotReceive().UpdateAsync(Arg.Any<Game>(), Arg.Any<CancellationToken>());
            await UnitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task ShouldUpdateGame_WithRequestValues_WhenGameExists()
        {
            var existingGame = CreateExistingGame();
            GamesRepository.GetByIdAsync(existingGame.Id, Arg.Any<CancellationToken>()).Returns(existingGame);
            var command = new UpdateGameCommand(existingGame.Id, "new-location", DateTime.UtcNow.AddDays(1), 90);
            var sut = CreateSut();

            var result = await sut.HandleAsync(command, TestContext.Current.CancellationToken);

            Assert.Equal(command.Location, result.Location);
            Assert.Equal(command.StartTime, result.StartTime);
            Assert.Equal(command.Duration, result.Duration);
        }

        [Fact]
        public async Task ShouldPersistChangesAndReturnTheUpdatedGame_WhenGameIsDirtyAfterUpdate()
        {
            var existingGame = CreateExistingGame();
            GamesRepository.GetByIdAsync(existingGame.Id, Arg.Any<CancellationToken>()).Returns(existingGame);
            var command = new UpdateGameCommand(existingGame.Id, "new-location", DateTime.UtcNow.AddDays(1), 90);
            var sut = CreateSut();

            var result = await sut.HandleAsync(command, TestContext.Current.CancellationToken);

            Assert.Same(existingGame, result);
            await GamesRepository.Received(1).UpdateAsync(existingGame, Arg.Any<CancellationToken>());
            await UnitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
        }
    }
}