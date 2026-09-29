using Teams.Core.Exceptions;
using Teams.Core.Models;
using Teams.Core.UseCases.Games.DeleteGame;
using Teams.Data.Models;
using Teams.Domain.Entities;
using Teams.Domain.Enums;

namespace Teams.Core.UnitTests.UseCases.Games.DeleteGame;

public static class DeleteGameCommandHandlerTests
{
    public class HandleAsync : UseCaseTestBase<DeleteGameCommand>
    {
        private static Game CreateExistingGame() =>
            new("organiser-id", "existing-location", DateTime.UtcNow, 60, 5);

        private static Invitation CreateOpenInvitation(Game game) =>
            new(game.Id, $"user-{Guid.NewGuid():N}", $"{Guid.NewGuid():N}@test.io") { Game = game };

        private DeleteGameCommandHandler CreateSut() =>
            new(UnitOfWork, ActorAccessor, new FakeLogger<DeleteGameCommandHandler>());

        private void StubOpenInvitations(string gameId, params Invitation[] invitations) =>
            InvitationsRepository.GetInvitationsAsync(
                gameId: gameId,
                userId: Arg.Any<string?>(),
                emailAddress: Arg.Any<string?>(),
                status: InvitationStatusEnum.Open,
                dateFilter: Arg.Any<DateFilter?>(),
                pagination: Arg.Any<PaginationFilter?>(),
                cancellationToken: Arg.Any<CancellationToken>())
                .Returns(invitations);

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
            StubOpenInvitations(existingGame.Id);
            var command = new DeleteGameCommand(existingGame.Id);
            var sut = CreateSut();

            var result = await sut.HandleAsync(command, TestContext.Current.CancellationToken);

            Assert.Same(existingGame, result);
            Assert.NotNull(result.DateDeleted);
            await GamesRepository.Received(1).UpdateAsync(existingGame, Arg.Any<CancellationToken>());
            await UnitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task ShouldFailOpenInvitationsAndPersistThem_WhenGameHasOpenInvitations()
        {
            var existingGame = CreateExistingGame();
            var firstInvitation = CreateOpenInvitation(existingGame);
            var secondInvitation = CreateOpenInvitation(existingGame);
            GamesRepository.GetByIdAsync(existingGame.Id, Arg.Any<CancellationToken>()).Returns(existingGame);
            StubOpenInvitations(existingGame.Id, firstInvitation, secondInvitation);
            var command = new DeleteGameCommand(existingGame.Id);
            var sut = CreateSut();

            await sut.HandleAsync(command, TestContext.Current.CancellationToken);

            Assert.Equal(InvitationStatusEnum.Failed, firstInvitation.Status);
            Assert.Equal("Game was deleted.", firstInvitation.ErrorMessage);
            Assert.Equal(InvitationStatusEnum.Failed, secondInvitation.Status);
            Assert.Equal("Game was deleted.", secondInvitation.ErrorMessage);
            await InvitationsRepository.Received(1).UpdateAsync(firstInvitation, Arg.Any<CancellationToken>());
            await InvitationsRepository.Received(1).UpdateAsync(secondInvitation, Arg.Any<CancellationToken>());
            await UnitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task ShouldNotTouchInvitations_WhenGameHasNoOpenInvitations()
        {
            var existingGame = CreateExistingGame();
            GamesRepository.GetByIdAsync(existingGame.Id, Arg.Any<CancellationToken>()).Returns(existingGame);
            StubOpenInvitations(existingGame.Id);
            var command = new DeleteGameCommand(existingGame.Id);
            var sut = CreateSut();

            await sut.HandleAsync(command, TestContext.Current.CancellationToken);

            await InvitationsRepository.DidNotReceive().UpdateAsync(Arg.Any<Invitation>(), Arg.Any<CancellationToken>());
        }
    }
}