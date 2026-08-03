using Teams.Core.Exceptions;
using Teams.Core.UseCases.Players.CreateDummyPlayer;
using Teams.Domain.Entities;

namespace Teams.Core.UnitTests.UseCases.Players.CreateDummyPlayer;

public static class CreateDummyPlayerCommandHandlerTests
{
    public class HandleAsync : UseCaseTestBase<CreateDummyPlayerCommand>
    {
        private static Game CreateExistingGame() => new("organiser-id", "location", DateTime.UtcNow, 60, 5);

        private static CreateDummyPlayerCommand CreateValidCommand(string gameId) =>
            new(gameId, "display-name", 1000);

        private CreateDummyPlayerCommandHandler CreateSut() =>
            new(UnitOfWork, Validator, new FakeLogger<CreateDummyPlayerCommandHandler>());

        [Fact]
        public async Task ShouldThrowCommandValidationException_WhenValidationFails()
        {
            SetupValidator(InvalidResult());
            var command = CreateValidCommand("game-id");
            var sut = CreateSut();

            await Assert.ThrowsAsync<CommandValidationException>(
                () => sut.HandleAsync(command, TestContext.Current.CancellationToken));
        }

        [Fact]
        public async Task ShouldNotLoadGame_WhenValidationFails()
        {
            SetupValidator(InvalidResult());
            var command = CreateValidCommand("game-id");
            var sut = CreateSut();

            await Assert.ThrowsAsync<CommandValidationException>(
                () => sut.HandleAsync(command, TestContext.Current.CancellationToken));

            await GamesRepository.DidNotReceive().GetByIdAsync(Arg.Any<string>(), Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task ShouldThrowNotFoundException_WhenGameDoesNotExist()
        {
            GamesRepository.GetByIdAsync("missing-game", Arg.Any<CancellationToken>()).Returns((Game?)null);
            var command = CreateValidCommand("missing-game");
            var sut = CreateSut();

            var exception = await Assert.ThrowsAsync<NotFoundException>(
                () => sut.HandleAsync(command, TestContext.Current.CancellationToken));

            Assert.Equal(nameof(Game), exception.ResourceType);
            Assert.Equal("missing-game", exception.ResourceIdentifier);
        }

        [Fact]
        public async Task ShouldNotCreatePlayer_WhenGameDoesNotExist()
        {
            GamesRepository.GetByIdAsync("missing-game", Arg.Any<CancellationToken>()).Returns((Game?)null);
            var command = CreateValidCommand("missing-game");
            var sut = CreateSut();

            await Assert.ThrowsAsync<NotFoundException>(
                () => sut.HandleAsync(command, TestContext.Current.CancellationToken));

            await PlayersRepository.DidNotReceive().CreateAsync(Arg.Any<Player>(), Arg.Any<CancellationToken>());
            await UnitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task ShouldCreateDummyPlayer_WithRequestValues_WhenGameExists()
        {
            var game = CreateExistingGame();
            GamesRepository.GetByIdAsync(game.Id, Arg.Any<CancellationToken>()).Returns(game);
            PlayersRepository.CreateAsync(Arg.Any<Player>(), Arg.Any<CancellationToken>())
                .Returns(callInfo => callInfo.Arg<Player>()!);
            var command = CreateValidCommand(game.Id);
            var sut = CreateSut();

            var result = await sut.HandleAsync(command, TestContext.Current.CancellationToken);

            Assert.Equal(game.Id, result.GameId);
            Assert.Equal(command.DisplayName, result.DisplayName);
            Assert.Equal(command.EstimatedRating, result.Rating);
            Assert.Null(result.UserId);
            Assert.Equal(Teams.Domain.Enums.PlayerTypeEnum.Dummy, result.Type);
        }

        [Fact]
        public async Task ShouldPersistChangesAndReturnTheCreatedPlayer_WhenGameExists()
        {
            var game = CreateExistingGame();
            GamesRepository.GetByIdAsync(game.Id, Arg.Any<CancellationToken>()).Returns(game);
            var command = CreateValidCommand(game.Id);
            var created = new Player(game, command.DisplayName, command.EstimatedRating);
            PlayersRepository.CreateAsync(Arg.Any<Player>(), Arg.Any<CancellationToken>()).Returns(created);
            var sut = CreateSut();

            var result = await sut.HandleAsync(command, TestContext.Current.CancellationToken);

            Assert.Same(created, result);
            await UnitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
        }
    }
}