using Teams.Core.Exceptions;
using Teams.Core.Models;
using Teams.Core.UseCases.Players.DeletePlayer;
using Teams.Domain.Entities;

namespace Teams.Core.UnitTests.UseCases.Players.DeletePlayer;

public static class DeletePlayerCommandHandlerTests
{
    public class HandleAsync : UseCaseTestBase<DeletePlayerCommand>
    {
        private static Game CreateExistingGame() =>
            new("organiser-id", "location", DateTime.UtcNow, 60, 5);

        private static Player CreateDummyPlayer(Game game) =>
            new(game, "dummy-display-name", 1000);

        private static Player CreateUserPlayer(Game game, User user) =>
            new(game, user);

        private DeletePlayerCommandHandler CreateSut() =>
            new(UnitOfWork, ActorAccessor, new FakeLogger<DeletePlayerCommandHandler>());

        [Fact]
        public async Task ShouldThrowNotFoundException_WhenPlayerDoesNotExist()
        {
            PlayersRepository.GetByIdAsync("missing-player", Arg.Any<CancellationToken>()).Returns((Player?)null);
            var command = new DeletePlayerCommand("missing-player");
            var sut = CreateSut();

            var exception = await Assert.ThrowsAsync<NotFoundException>(
                () => sut.HandleAsync(command, TestContext.Current.CancellationToken));

            Assert.Equal(nameof(Player), exception.ResourceType);
            Assert.Equal("missing-player", exception.ResourceIdentifier);
        }

        [Fact]
        public async Task ShouldThrowAccessDeniedExceptionAndNotPersistChanges_WhenDummyPlayerAndActorIsNotOrganiser()
        {
            var game = CreateExistingGame();
            var player = CreateDummyPlayer(game);
            PlayersRepository.GetByIdAsync(player.Id, Arg.Any<CancellationToken>()).Returns(player);
            ActorAccessor.Current.Returns(new Actor("some-other-actor", "tag", "display-name"));
            var command = new DeletePlayerCommand(player.Id);
            var sut = CreateSut();

            await Assert.ThrowsAsync<AccessDeniedException>(
                () => sut.HandleAsync(command, TestContext.Current.CancellationToken));

            await PlayersRepository.DidNotReceive().UpdateAsync(Arg.Any<Player>(), Arg.Any<CancellationToken>());
            await UnitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task ShouldPersistDeletionAndReturnThePlayer_WhenDummyPlayerAndActorIsOrganiser()
        {
            var game = CreateExistingGame();
            var player = CreateDummyPlayer(game);
            PlayersRepository.GetByIdAsync(player.Id, Arg.Any<CancellationToken>()).Returns(player);
            var command = new DeletePlayerCommand(player.Id);
            var sut = CreateSut();

            var result = await sut.HandleAsync(command, TestContext.Current.CancellationToken);

            Assert.Same(player, result);
            Assert.NotNull(result.DateDeleted);
            await PlayersRepository.Received(1).UpdateAsync(player, Arg.Any<CancellationToken>());
            await UnitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task ShouldThrowAccessDeniedExceptionAndNotPersistChanges_WhenUserPlayerAndActorIsNeitherOrganiserNorSelf()
        {
            var game = CreateExistingGame();
            var user = new User("display-name", "external-id", "user@example.com", null);
            var player = CreateUserPlayer(game, user);
            PlayersRepository.GetByIdAsync(player.Id, Arg.Any<CancellationToken>()).Returns(player);
            ActorAccessor.Current.Returns(new Actor("some-other-actor", "tag", "display-name"));
            var command = new DeletePlayerCommand(player.Id);
            var sut = CreateSut();

            await Assert.ThrowsAsync<AccessDeniedException>(
                () => sut.HandleAsync(command, TestContext.Current.CancellationToken));

            await PlayersRepository.DidNotReceive().UpdateAsync(Arg.Any<Player>(), Arg.Any<CancellationToken>());
            await UnitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task ShouldPersistDeletionAndReturnThePlayer_WhenUserPlayerAndActorIsOrganiser()
        {
            var game = CreateExistingGame();
            var user = new User("display-name", "external-id", "user@example.com", null);
            var player = CreateUserPlayer(game, user);
            PlayersRepository.GetByIdAsync(player.Id, Arg.Any<CancellationToken>()).Returns(player);
            var command = new DeletePlayerCommand(player.Id);
            var sut = CreateSut();

            var result = await sut.HandleAsync(command, TestContext.Current.CancellationToken);

            Assert.Same(player, result);
            Assert.NotNull(result.DateDeleted);
            await PlayersRepository.Received(1).UpdateAsync(player, Arg.Any<CancellationToken>());
            await UnitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task ShouldPersistDeletionAndReturnThePlayer_WhenUserPlayerAndActorIsSelf()
        {
            var game = CreateExistingGame();
            var user = new User("display-name", "external-id", "user@example.com", null);
            var player = CreateUserPlayer(game, user);
            PlayersRepository.GetByIdAsync(player.Id, Arg.Any<CancellationToken>()).Returns(player);
            ActorAccessor.Current.Returns(new Actor(user.Id, user.Tag, user.DisplayName));
            var command = new DeletePlayerCommand(player.Id);
            var sut = CreateSut();

            var result = await sut.HandleAsync(command, TestContext.Current.CancellationToken);

            Assert.Same(player, result);
            Assert.NotNull(result.DateDeleted);
            await PlayersRepository.Received(1).UpdateAsync(player, Arg.Any<CancellationToken>());
            await UnitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
        }
    }
}