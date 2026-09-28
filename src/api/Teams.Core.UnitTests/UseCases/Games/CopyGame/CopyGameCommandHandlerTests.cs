using Teams.Core.Exceptions;
using Teams.Core.Models;
using Teams.Core.UseCases.Games.CopyGame;
using Teams.Core.UseCases.Invitations.CreateInvitations;
using Teams.Domain.Entities;
using Teams.Domain.Enums;

namespace Teams.Core.UnitTests.UseCases.Games.CopyGame;

public static class CopyGameCommandHandlerTests
{
    public class HandleAsync : UseCaseTestBase<CopyGameCommand>
    {
        private static Game CreateFinishedGame()
        {
            var game = new Game("organiser-id", "location", DateTime.UtcNow, 60, 5);
            game.SetResult(GameTeamEnum.Home);
            return game;
        }

        private static Player AddDummyPlayer(Game game, GameTeamEnum team, int rating, int? ratingChange)
        {
            var player = new Player(game, $"dummy-{Guid.NewGuid():N}", rating);
            player.AssignTeam(team, null);
            if (ratingChange is not null)
                player.SetRatingChange(rating, ratingChange.Value, 1);
            game.Players.Add(player);
            return player;
        }

        private static Player AddUserPlayer(Game game, GameTeamEnum team, User user)
        {
            var player = new Player(game, user);
            player.AssignTeam(team, null);
            game.Players.Add(player);
            return player;
        }

        private CopyGameCommandHandler CreateSut() =>
            new(UnitOfWork, ActorAccessor, EventPublisher, new FakeLogger<CopyGameCommandHandler>());

        [Fact]
        public async Task ShouldThrowNotFoundException_WhenGameDoesNotExist()
        {
            GamesRepository.GetByIdAsync("missing-game", Arg.Any<CancellationToken>()).Returns((Game?)null);
            var command = new CopyGameCommand("missing-game", DateTime.UtcNow);
            var sut = CreateSut();

            var exception = await Assert.ThrowsAsync<NotFoundException>(
                () => sut.HandleAsync(command, TestContext.Current.CancellationToken));

            Assert.Equal(nameof(Game), exception.ResourceType);
            Assert.Equal("missing-game", exception.ResourceIdentifier);
        }

        [Fact]
        public async Task ShouldThrowAccessDeniedExceptionAndNotPersistChanges_WhenActorIsNotOrganiser()
        {
            var game = CreateFinishedGame();
            GamesRepository.GetByIdAsync(game.Id, Arg.Any<CancellationToken>()).Returns(game);
            ActorAccessor.Current.Returns(new Actor("some-other-actor", "tag", "display-name"));
            var command = new CopyGameCommand(game.Id, DateTime.UtcNow);
            var sut = CreateSut();

            await Assert.ThrowsAsync<AccessDeniedException>(
                () => sut.HandleAsync(command, TestContext.Current.CancellationToken));

            await GamesRepository.DidNotReceive().CreateAsync(Arg.Any<Game>(), Arg.Any<CancellationToken>());
            await UnitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task ShouldThrowCommandValidationException_WhenSourceGameIsNotFinished()
        {
            var game = new Game("organiser-id", "location", DateTime.UtcNow, 60, 5);
            GamesRepository.GetByIdAsync(game.Id, Arg.Any<CancellationToken>()).Returns(game);
            var command = new CopyGameCommand(game.Id, DateTime.UtcNow);
            var sut = CreateSut();

            await Assert.ThrowsAsync<CommandValidationException>(
                () => sut.HandleAsync(command, TestContext.Current.CancellationToken));

            await GamesRepository.DidNotReceive().CreateAsync(Arg.Any<Game>(), Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task ShouldCreateNewScheduledGame_CopyingLocationDurationAndTeamSize()
        {
            var game = CreateFinishedGame();
            GamesRepository.GetByIdAsync(game.Id, Arg.Any<CancellationToken>()).Returns(game);
            var newStartTime = DateTime.UtcNow.AddDays(7);
            var command = new CopyGameCommand(game.Id, newStartTime);
            var sut = CreateSut();

            var result = await sut.HandleAsync(command, TestContext.Current.CancellationToken);

            Assert.NotEqual(game.Id, result.Id);
            Assert.Equal(GameStatusEnum.Scheduled, result.Status);
            Assert.Equal(game.Location, result.Location);
            Assert.Equal(game.Duration, result.Duration);
            Assert.Equal(game.TeamSize, result.TeamSize);
            Assert.Equal(newStartTime, result.StartTime);
            await UnitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task ShouldInviteUserPlayer_RatherThanAddingThemDirectly()
        {
            var game = CreateFinishedGame();
            var user = new User("display-name", "external-id", "user@example.com", null);
            AddUserPlayer(game, GameTeamEnum.Home, user);
            GamesRepository.GetByIdAsync(game.Id, Arg.Any<CancellationToken>()).Returns(game);
            var command = new CopyGameCommand(game.Id, DateTime.UtcNow);
            var sut = CreateSut();

            var result = await sut.HandleAsync(command, TestContext.Current.CancellationToken);

            await InvitationsRepository.Received(1).CreateAsync(
                Arg.Is<Invitation>(i => i!.GameId == result.Id && i.UserId == user.Id && i.EmailAddress == user.EmailAddress),
                Arg.Any<CancellationToken>());
            await PlayersRepository.DidNotReceive().CreateAsync(
                Arg.Is<Player>(p => p!.UserId == user.Id), Arg.Any<CancellationToken>());
            await EventPublisher.Received(1).PublishEventsAsync(
                Arg.Is<IEnumerable<InvitationCreatedEvent>>(events => events!.Any(e => e.GameId == result.Id && e.UserId == user.Id)),
                Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task ShouldSkipPlayer_WhenLinkedUserHasBeenDeleted()
        {
            var game = CreateFinishedGame();
            var player = new Player(game.Id, "deleted-user-id", 1000, PlayerTypeEnum.User, GameTeamEnum.Home);
            game.Players.Add(player);
            GamesRepository.GetByIdAsync(game.Id, Arg.Any<CancellationToken>()).Returns(game);
            var command = new CopyGameCommand(game.Id, DateTime.UtcNow);
            var sut = CreateSut();

            await sut.HandleAsync(command, TestContext.Current.CancellationToken);

            await InvitationsRepository.DidNotReceive().CreateAsync(Arg.Any<Invitation>(), Arg.Any<CancellationToken>());
            await PlayersRepository.DidNotReceive().CreateAsync(Arg.Any<Player>(), Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task ShouldCopyDummyPlayer_WithRatingPlusRatingChangeAndUnassignedTeam()
        {
            var game = CreateFinishedGame();
            AddDummyPlayer(game, GameTeamEnum.Home, 1000, 15);
            GamesRepository.GetByIdAsync(game.Id, Arg.Any<CancellationToken>()).Returns(game);
            var command = new CopyGameCommand(game.Id, DateTime.UtcNow);
            var sut = CreateSut();

            await sut.HandleAsync(command, TestContext.Current.CancellationToken);

            await PlayersRepository.Received(1).CreateAsync(
                Arg.Is<Player>(p => p!.UserId == null && p.Rating == 1015 && p.Team == GameTeamEnum.None),
                Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task ShouldCopyDummyPlayer_WithUnchangedRating_WhenPlayerWasUnassigned()
        {
            var game = CreateFinishedGame();
            AddDummyPlayer(game, GameTeamEnum.None, 1000, null);
            GamesRepository.GetByIdAsync(game.Id, Arg.Any<CancellationToken>()).Returns(game);
            var command = new CopyGameCommand(game.Id, DateTime.UtcNow);
            var sut = CreateSut();

            await sut.HandleAsync(command, TestContext.Current.CancellationToken);

            await PlayersRepository.Received(1).CreateAsync(
                Arg.Is<Player>(p => p!.Rating == 1000), Arg.Any<CancellationToken>());
        }
    }
}