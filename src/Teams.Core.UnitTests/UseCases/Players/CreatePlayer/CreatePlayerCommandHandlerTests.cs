using Teams.Core.Exceptions;
using Teams.Core.UseCases.Players.CreatePlayer;
using Teams.Domain.Entities;

namespace Teams.Core.UnitTests.UseCases.Players.CreatePlayer;

public static class CreatePlayerCommandHandlerTests
{
    public class HandleAsync : UseCaseTestBase<CreatePlayerCommand>
    {
        private static Game CreateExistingGame() => new("organiser-id", "location", DateTime.UtcNow, 60, 5);

        private static User CreateExistingUser() =>
            new("display-name", "external-id", "user@example.com", null);

        private CreatePlayerCommandHandler CreateSut() =>
            new(UnitOfWork, new FakeLogger<CreatePlayerCommandHandler>());

        [Fact]
        public async Task ShouldThrowNotFoundException_WhenGameDoesNotExist()
        {
            GamesRepository.GetByIdAsync("missing-game", Arg.Any<CancellationToken>()).Returns((Game?)null);
            var command = new CreatePlayerCommand("missing-game", "user-id");
            var sut = CreateSut();

            var exception = await Assert.ThrowsAsync<NotFoundException>(
                () => sut.HandleAsync(command, TestContext.Current.CancellationToken));

            Assert.Equal(nameof(Game), exception.ResourceType);
            Assert.Equal("missing-game", exception.ResourceIdentifier);
        }

        [Fact]
        public async Task ShouldThrowCommandValidationException_WhenUserAlreadyAssociatedWithGame()
        {
            var game = CreateExistingGame();
            var existingPlayer = new Player(game.Id, "user-id", "existing-display-name", 1000,
                Teams.Domain.Enums.PlayerTypeEnum.User, Teams.Domain.Enums.GameTeamEnum.None);
            game.Players.Add(existingPlayer);
            GamesRepository.GetByIdAsync(game.Id, Arg.Any<CancellationToken>()).Returns(game);
            var command = new CreatePlayerCommand(game.Id, "user-id");
            var sut = CreateSut();

            var exception = await Assert.ThrowsAsync<CommandValidationException>(
                () => sut.HandleAsync(command, TestContext.Current.CancellationToken));

            Assert.Contains(exception.Errors, error => error.PropertyName == nameof(CreatePlayerCommand.UserId));
        }

        [Fact]
        public async Task ShouldNotLoadUser_WhenUserAlreadyAssociatedWithGame()
        {
            var game = CreateExistingGame();
            var existingPlayer = new Player(game.Id, "user-id", "existing-display-name", 1000,
                Teams.Domain.Enums.PlayerTypeEnum.User, Teams.Domain.Enums.GameTeamEnum.None);
            game.Players.Add(existingPlayer);
            GamesRepository.GetByIdAsync(game.Id, Arg.Any<CancellationToken>()).Returns(game);
            var command = new CreatePlayerCommand(game.Id, "user-id");
            var sut = CreateSut();

            await Assert.ThrowsAsync<CommandValidationException>(
                () => sut.HandleAsync(command, TestContext.Current.CancellationToken));

            await UsersRepository.DidNotReceive().GetByIdAsync(Arg.Any<string>(), Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task ShouldThrowNotFoundException_WhenUserDoesNotExist()
        {
            var game = CreateExistingGame();
            GamesRepository.GetByIdAsync(game.Id, Arg.Any<CancellationToken>()).Returns(game);
            UsersRepository.GetByIdAsync("missing-user", Arg.Any<CancellationToken>()).Returns((User?)null);
            var command = new CreatePlayerCommand(game.Id, "missing-user");
            var sut = CreateSut();

            var exception = await Assert.ThrowsAsync<NotFoundException>(
                () => sut.HandleAsync(command, TestContext.Current.CancellationToken));

            Assert.Equal(nameof(User), exception.ResourceType);
            Assert.Equal("missing-user", exception.ResourceIdentifier);
        }

        [Fact]
        public async Task ShouldCreatePlayer_FromGameAndUser_WhenBothExist()
        {
            var game = CreateExistingGame();
            var user = CreateExistingUser();
            GamesRepository.GetByIdAsync(game.Id, Arg.Any<CancellationToken>()).Returns(game);
            UsersRepository.GetByIdAsync(user.Id, Arg.Any<CancellationToken>()).Returns(user);
            PlayersRepository.CreateAsync(Arg.Any<Player>(), Arg.Any<CancellationToken>())
                .Returns(callInfo => callInfo.Arg<Player>()!);
            var command = new CreatePlayerCommand(game.Id, user.Id);
            var sut = CreateSut();

            var result = await sut.HandleAsync(command, TestContext.Current.CancellationToken);

            Assert.Equal(game.Id, result.GameId);
            Assert.Equal(user.Id, result.UserId);
            Assert.Equal(user.Tag, result.DisplayName);
            Assert.Equal(user.Rating, result.Rating);
        }

        [Fact]
        public async Task ShouldPersistChangesAndReturnTheCreatedPlayer_WhenBothExist()
        {
            var game = CreateExistingGame();
            var user = CreateExistingUser();
            GamesRepository.GetByIdAsync(game.Id, Arg.Any<CancellationToken>()).Returns(game);
            UsersRepository.GetByIdAsync(user.Id, Arg.Any<CancellationToken>()).Returns(user);
            var command = new CreatePlayerCommand(game.Id, user.Id);
            var created = new Player(game, user);
            PlayersRepository.CreateAsync(Arg.Any<Player>(), Arg.Any<CancellationToken>()).Returns(created);
            var sut = CreateSut();

            var result = await sut.HandleAsync(command, TestContext.Current.CancellationToken);

            Assert.Same(created, result);
            await UnitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
        }
    }
}