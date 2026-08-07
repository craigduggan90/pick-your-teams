using Teams.Core.Exceptions;
using Teams.Core.Models;
using Teams.Core.UseCases.Games.SetTeams;
using Teams.Domain.Entities;
using Teams.Domain.Enums;

namespace Teams.Core.UnitTests.UseCases.Games.SetTeams;

public static class SetTeamsCommandHandlerTests
{
    public class HandleAsync : UseCaseTestBase<SetTeamsCommand>
    {
        private static Game CreateExistingGame() =>
            new("organiser-id", "location", DateTime.UtcNow, 60, 3); // TeamSize 3, MaxPlayers 6

        private static Player AddPlayer(Game game, int rating = 1000)
        {
            var player = new Player(game, $"dummy-{Guid.NewGuid():N}", rating);
            game.Players.Add(player);
            return player;
        }

        private SetTeamsCommandHandler CreateSut() =>
            new(UnitOfWork, ActorAccessor, Validator, new FakeLogger<SetTeamsCommandHandler>());

        [Fact]
        public async Task ShouldThrowCommandValidationExceptionAndNotLoadGame_WhenValidationFails()
        {
            SetupValidator(InvalidResult());
            var command = new SetTeamsCommand("game-id", [], []);
            var sut = CreateSut();

            await Assert.ThrowsAsync<CommandValidationException>(
                () => sut.HandleAsync(command, TestContext.Current.CancellationToken));

            await GamesRepository.DidNotReceive().GetByIdAsync(Arg.Any<string>(), Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task ShouldThrowNotFoundException_WhenGameDoesNotExist()
        {
            GamesRepository.GetByIdAsync("missing-game", Arg.Any<CancellationToken>()).Returns((Game?)null);
            var command = new SetTeamsCommand("missing-game", [], []);
            var sut = CreateSut();

            var exception = await Assert.ThrowsAsync<NotFoundException>(
                () => sut.HandleAsync(command, TestContext.Current.CancellationToken));

            Assert.Equal(nameof(Game), exception.ResourceType);
            Assert.Equal("missing-game", exception.ResourceIdentifier);
        }

        [Fact]
        public async Task ShouldThrowAccessDeniedExceptionAndNotPersistChanges_WhenActorIsNotOrganiser()
        {
            var game = CreateExistingGame();
            var home = AddPlayer(game);
            GamesRepository.GetByIdAsync(game.Id, Arg.Any<CancellationToken>()).Returns(game);
            ActorAccessor.Current.Returns(new Actor("some-other-actor", "tag", "display-name"));
            var command = new SetTeamsCommand(game.Id, [home.Id], []);
            var sut = CreateSut();

            await Assert.ThrowsAsync<AccessDeniedException>(
                () => sut.HandleAsync(command, TestContext.Current.CancellationToken));

            await PlayersRepository.DidNotReceive().UpdateAsync(Arg.Any<Player>(), Arg.Any<CancellationToken>());
            await UnitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task ShouldThrowRequestHandlerException_WhenGameIsFinished()
        {
            var game = CreateExistingGame();
            AddPlayer(game);
            AddPlayer(game);
            game.SetResult(GameTeamEnum.Home);
            GamesRepository.GetByIdAsync(game.Id, Arg.Any<CancellationToken>()).Returns(game);
            var command = new SetTeamsCommand(game.Id, [], []);
            var sut = CreateSut();

            await Assert.ThrowsAsync<RequestHandlerException>(
                () => sut.HandleAsync(command, TestContext.Current.CancellationToken));
        }

        [Fact]
        public async Task ShouldThrowRequestHandlerException_WhenTooManyHomePlayersProvided()
        {
            var game = CreateExistingGame(); // TeamSize 3
            GamesRepository.GetByIdAsync(game.Id, Arg.Any<CancellationToken>()).Returns(game);
            var command = new SetTeamsCommand(game.Id, ["p1", "p2", "p3", "p4"], []);
            var sut = CreateSut();

            await Assert.ThrowsAsync<RequestHandlerException>(
                () => sut.HandleAsync(command, TestContext.Current.CancellationToken));
        }

        [Fact]
        public async Task ShouldThrowRequestHandlerException_WhenTooManyAwayPlayersProvided()
        {
            var game = CreateExistingGame(); // TeamSize 3
            GamesRepository.GetByIdAsync(game.Id, Arg.Any<CancellationToken>()).Returns(game);
            var command = new SetTeamsCommand(game.Id, [], ["p1", "p2", "p3", "p4"]);
            var sut = CreateSut();

            await Assert.ThrowsAsync<RequestHandlerException>(
                () => sut.HandleAsync(command, TestContext.Current.CancellationToken));
        }

        [Fact]
        public async Task ShouldThrowNotFoundException_WhenHomePlayerIdDoesNotExistOnGame()
        {
            var game = CreateExistingGame();
            GamesRepository.GetByIdAsync(game.Id, Arg.Any<CancellationToken>()).Returns(game);
            var command = new SetTeamsCommand(game.Id, ["missing-player"], []);
            var sut = CreateSut();

            var exception = await Assert.ThrowsAsync<NotFoundException>(
                () => sut.HandleAsync(command, TestContext.Current.CancellationToken));

            Assert.Equal(nameof(Player), exception.ResourceType);
            Assert.Equal("missing-player", exception.ResourceIdentifier);
        }

        [Fact]
        public async Task ShouldThrowNotFoundException_WhenAwayPlayerIdDoesNotExistOnGame()
        {
            var game = CreateExistingGame();
            GamesRepository.GetByIdAsync(game.Id, Arg.Any<CancellationToken>()).Returns(game);
            var command = new SetTeamsCommand(game.Id, [], ["missing-player"]);
            var sut = CreateSut();

            var exception = await Assert.ThrowsAsync<NotFoundException>(
                () => sut.HandleAsync(command, TestContext.Current.CancellationToken));

            Assert.Equal(nameof(Player), exception.ResourceType);
            Assert.Equal("missing-player", exception.ResourceIdentifier);
        }

        [Fact]
        public async Task ShouldAssignPlayersToRequestedTeams_AndUnassignEveryoneElse()
        {
            var game = CreateExistingGame();
            var home = AddPlayer(game);
            var away = AddPlayer(game);
            var leftoverAlreadyOnHome = AddPlayer(game);
            leftoverAlreadyOnHome.AssignTeam(GameTeamEnum.Home, null);
            GamesRepository.GetByIdAsync(game.Id, Arg.Any<CancellationToken>()).Returns(game);
            var command = new SetTeamsCommand(game.Id, [home.Id], [away.Id]);
            var sut = CreateSut();

            await sut.HandleAsync(command, TestContext.Current.CancellationToken);

            Assert.Equal(GameTeamEnum.Home, home.Team);
            Assert.Equal(GameTeamEnum.Away, away.Team);
            Assert.Equal(GameTeamEnum.None, leftoverAlreadyOnHome.Team);
        }

        [Fact]
        public async Task ShouldUpdateTeamRatings_ToReflectNewAssignments()
        {
            var game = CreateExistingGame();
            var home1 = AddPlayer(game);
            var home2 = AddPlayer(game, 1100);
            var away1 = AddPlayer(game, 950);
            GamesRepository.GetByIdAsync(game.Id, Arg.Any<CancellationToken>()).Returns(game);
            var command = new SetTeamsCommand(game.Id, [home1.Id, home2.Id], [away1.Id]);
            var sut = CreateSut();

            var result = await sut.HandleAsync(command, TestContext.Current.CancellationToken);

            Assert.Equal(2100, result.HomeTeamRating);
            Assert.Equal(950, result.AwayTeamRating);
        }

        [Fact]
        public async Task ShouldPersistChangesAndReturnTheGame_WhenTeamsAreSet()
        {
            var game = CreateExistingGame();
            var home = AddPlayer(game);
            var away = AddPlayer(game);
            GamesRepository.GetByIdAsync(game.Id, Arg.Any<CancellationToken>()).Returns(game);
            var command = new SetTeamsCommand(game.Id, [home.Id], [away.Id]);
            var sut = CreateSut();

            var result = await sut.HandleAsync(command, TestContext.Current.CancellationToken);

            Assert.Same(game, result);
            await GamesRepository.Received(1).UpdateAsync(game, Arg.Any<CancellationToken>());
            await PlayersRepository.Received(1).UpdateAsync(home, Arg.Any<CancellationToken>());
            await PlayersRepository.Received(1).UpdateAsync(away, Arg.Any<CancellationToken>());
            await UnitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task ShouldNotUpdatePlayer_WhenTeamAndRatingAreUnchanged()
        {
            var game = CreateExistingGame();
            var home = new Player(game.Id, null, "dummy", 1000, PlayerTypeEnum.Dummy, GameTeamEnum.Home) { Game = game };
            game.Players.Add(home);
            GamesRepository.GetByIdAsync(game.Id, Arg.Any<CancellationToken>()).Returns(game);
            var command = new SetTeamsCommand(game.Id, [home.Id], []);
            var sut = CreateSut();

            await sut.HandleAsync(command, TestContext.Current.CancellationToken);

            await PlayersRepository.DidNotReceive().UpdateAsync(Arg.Any<Player>(), Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task ShouldUnassignAllPlayers_WhenBothTeamListsAreEmpty()
        {
            var game = CreateExistingGame();
            var home = AddPlayer(game);
            home.AssignTeam(GameTeamEnum.Home, null);
            var away = AddPlayer(game);
            away.AssignTeam(GameTeamEnum.Away, null);
            GamesRepository.GetByIdAsync(game.Id, Arg.Any<CancellationToken>()).Returns(game);
            var command = new SetTeamsCommand(game.Id, [], []);
            var sut = CreateSut();

            await sut.HandleAsync(command, TestContext.Current.CancellationToken);

            Assert.Equal(GameTeamEnum.None, home.Team);
            Assert.Equal(GameTeamEnum.None, away.Team);
            Assert.Equal(0, game.HomeTeamRating);
            Assert.Equal(0, game.AwayTeamRating);
        }
    }
}