using Teams.Core.Exceptions;
using Teams.Core.Models;
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
            new(UnitOfWork, ActorAccessor, new FakeLogger<CreatePlayerCommandHandler>());

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
        public async Task ShouldThrowAccessDeniedExceptionAndNotCreatePlayer_WhenActorIsNeitherOrganiserNorSubjectUser()
        {
            var game = CreateExistingGame();
            GamesRepository.GetByIdAsync(game.Id, Arg.Any<CancellationToken>()).Returns(game);
            ActorAccessor.Current.Returns(new Actor("some-other-actor", "tag", "display-name"));
            var command = new CreatePlayerCommand(game.Id, "user-id");
            var sut = CreateSut();

            await Assert.ThrowsAsync<AccessDeniedException>(
                () => sut.HandleAsync(command, TestContext.Current.CancellationToken));

            await PlayersRepository.DidNotReceive().CreateAsync(Arg.Any<Player>(), Arg.Any<CancellationToken>());
            await UnitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task ShouldThrowCommandValidationException_WhenUserAlreadyAssociatedWithGame_AndActorIsOrganiser()
        {
            var game = CreateExistingGame();
            var existingPlayer = new Player(
                gameId: game.Id,
                userId: "user-id",
                rating: 1000,
                type: Domain.Enums.PlayerTypeEnum.User,
                team: Domain.Enums.GameTeamEnum.None)
            {
                DisplayName = "existing-display-name"
            };

            game.Players.Add(existingPlayer);
            GamesRepository.GetByIdAsync(game.Id, Arg.Any<CancellationToken>()).Returns(game);
            var command = new CreatePlayerCommand(game.Id, "user-id");
            var sut = CreateSut();

            var exception = await Assert.ThrowsAsync<CommandValidationException>(
                () => sut.HandleAsync(command, TestContext.Current.CancellationToken));

            Assert.Contains(exception.Errors, error => error.PropertyName == nameof(CreatePlayerCommand.UserId));
        }

        [Fact]
        public async Task ShouldThrowCommandValidationException_WhenUserAlreadyAssociatedWithGame_AndActorIsSelf()
        {
            var game = CreateExistingGame();
            var existingPlayer = new Player(
                gameId: game.Id,
                userId: "user-id",
                rating: 1000,
                type: Domain.Enums.PlayerTypeEnum.User,
                team: Domain.Enums.GameTeamEnum.None)
            {
                DisplayName = "existing-display-name"
            };

            game.Players.Add(existingPlayer);
            GamesRepository.GetByIdAsync(game.Id, Arg.Any<CancellationToken>()).Returns(game);
            ActorAccessor.Current.Returns(new Actor("user-id", "tag", "display-name"));
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
            var existingPlayer = new Player(
                gameId: game.Id,
                userId: "user-id",
                rating: 1000,
                type: Domain.Enums.PlayerTypeEnum.User,
                team: Domain.Enums.GameTeamEnum.None)
            {
                DisplayName = "existing-display-name"
            };

            game.Players.Add(existingPlayer);
            GamesRepository.GetByIdAsync(game.Id, Arg.Any<CancellationToken>()).Returns(game);
            var command = new CreatePlayerCommand(game.Id, "user-id");
            var sut = CreateSut();

            await Assert.ThrowsAsync<CommandValidationException>(
                () => sut.HandleAsync(command, TestContext.Current.CancellationToken));

            await UsersRepository.DidNotReceive().GetByIdAsync(Arg.Any<string>(), Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task ShouldThrowNotFoundException_WhenUserDoesNotExist_AndActorIsOrganiser()
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
        public async Task ShouldCreatePlayer_FromGameAndUser_WhenActorIsOrganiser()
        {
            var game = CreateExistingGame();
            var user = CreateExistingUser();
            GamesRepository.GetByIdAsync(game.Id, Arg.Any<CancellationToken>()).Returns(game);
            UsersRepository.GetByIdAsync(user.Id, Arg.Any<CancellationToken>()).Returns(user);
            PlayersRepository.CreateAsync(Arg.Any<Player>(), Arg.Any<CancellationToken>())
                .Returns(callInfo => callInfo.ArgAt<Player>(0));
            var command = new CreatePlayerCommand(game.Id, user.Id);
            var sut = CreateSut();

            var result = await sut.HandleAsync(command, TestContext.Current.CancellationToken);

            Assert.Equal(game.Id, result.GameId);
            Assert.Equal(user.Id, result.UserId);
            Assert.Equal(user.Tag, result.GetDisplayName());
            Assert.Equal(user.Rating, result.Rating);
        }

        [Fact]
        public async Task ShouldCreatePlayer_FromGameAndUser_WhenActorIsSelf()
        {
            var game = CreateExistingGame();
            var user = CreateExistingUser();
            GamesRepository.GetByIdAsync(game.Id, Arg.Any<CancellationToken>()).Returns(game);
            UsersRepository.GetByIdAsync(user.Id, Arg.Any<CancellationToken>()).Returns(user);
            PlayersRepository.CreateAsync(Arg.Any<Player>(), Arg.Any<CancellationToken>())
                .Returns(callInfo => callInfo.ArgAt<Player>(0));
            ActorAccessor.Current.Returns(new Actor(user.Id, user.Tag, user.DisplayName));
            var command = new CreatePlayerCommand(game.Id, user.Id);
            var sut = CreateSut();

            var result = await sut.HandleAsync(command, TestContext.Current.CancellationToken);

            Assert.Equal(game.Id, result.GameId);
            Assert.Equal(user.Id, result.UserId);
        }

        [Fact]
        public async Task ShouldPersistChangesAndReturnTheCreatedPlayer_WhenActorIsOrganiser()
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