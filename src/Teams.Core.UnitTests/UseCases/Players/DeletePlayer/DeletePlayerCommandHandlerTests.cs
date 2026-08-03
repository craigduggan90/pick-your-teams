using Teams.Core.Exceptions;
using Teams.Core.UseCases.Players.DeletePlayer;
using Teams.Domain.Entities;

namespace Teams.Core.UnitTests.UseCases.Players.DeletePlayer;

public static class DeletePlayerCommandHandlerTests
{
    public class HandleAsync : UseCaseTestBase<DeletePlayerCommand>
    {
        private static Player CreateExistingPlayer() =>
            new("game-id", null, "display-name", 1000, Teams.Domain.Enums.PlayerTypeEnum.Dummy, Teams.Domain.Enums.GameTeamEnum.None);

        private DeletePlayerCommandHandler CreateSut() =>
            new(UnitOfWork, new FakeLogger<DeletePlayerCommandHandler>());

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
        public async Task ShouldPersistDeletionAndReturnThePlayer_WhenPlayerExists()
        {
            var existingPlayer = CreateExistingPlayer();
            PlayersRepository.GetByIdAsync(existingPlayer.Id, Arg.Any<CancellationToken>()).Returns(existingPlayer);
            var command = new DeletePlayerCommand(existingPlayer.Id);
            var sut = CreateSut();

            var result = await sut.HandleAsync(command, TestContext.Current.CancellationToken);

            Assert.Same(existingPlayer, result);
            Assert.NotNull(result.DateDeleted);
            await PlayersRepository.Received(1).UpdateAsync(existingPlayer, Arg.Any<CancellationToken>());
            await UnitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
        }
    }
}