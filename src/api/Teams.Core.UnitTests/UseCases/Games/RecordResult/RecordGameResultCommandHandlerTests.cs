using Teams.Core.Exceptions;
using Teams.Core.UseCases.Games.RecordResult;
using Teams.Domain.Entities;
using Teams.Domain.Enums;

namespace Teams.Core.UnitTests.UseCases.Games.RecordGameResult;

public static class RecordGameResultCommandHandlerTests
{
    public class HandleAsync : UseCaseTestBase<RecordGameResultCommand>
    {
        private static Game CreateGame() => new("organiser-id", "location", DateTime.UtcNow, 60, 2);

        private static Player AddDummyPlayer(Game game, GameTeamEnum team, int rating)
        {
            var player = new Player(game, $"dummy-{Guid.NewGuid():N}", rating);
            player.AssignTeam(team, null);
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

        private RecordGameResultCommandHandler CreateSut() =>
            new(UnitOfWork, Validator, new FakeLogger<RecordGameResultCommandHandler>());

        [Fact]
        public async Task ShouldThrowCommandValidationException_WhenValidationFails()
        {
            SetupValidator(InvalidResult());
            var command = new RecordGameResultCommand("game-id", "Home");
            var sut = CreateSut();

            await Assert.ThrowsAsync<CommandValidationException>(
                () => sut.HandleAsync(command, TestContext.Current.CancellationToken));
        }

        [Fact]
        public async Task ShouldNotLoadGame_WhenValidationFails()
        {
            SetupValidator(InvalidResult());
            var command = new RecordGameResultCommand("game-id", "Home");
            var sut = CreateSut();

            await Assert.ThrowsAsync<CommandValidationException>(
                () => sut.HandleAsync(command, TestContext.Current.CancellationToken));

            await GamesRepository.DidNotReceive().GetByIdAsync(Arg.Any<string>(), Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task ShouldThrowNotFoundException_WhenGameDoesNotExist()
        {
            GamesRepository.GetByIdAsync("missing-game", Arg.Any<CancellationToken>()).Returns((Game?)null);
            var command = new RecordGameResultCommand("missing-game", "Home");
            var sut = CreateSut();

            var exception = await Assert.ThrowsAsync<NotFoundException>(
                () => sut.HandleAsync(command, TestContext.Current.CancellationToken));

            Assert.Equal(nameof(Game), exception.ResourceType);
            Assert.Equal("missing-game", exception.ResourceIdentifier);
        }

        [Fact]
        public async Task ShouldSetResultAndPersistGame_WhenGameExists()
        {
            var game = CreateGame();
            AddDummyPlayer(game, GameTeamEnum.Home, 1000);
            AddDummyPlayer(game, GameTeamEnum.Away, 1000);
            GamesRepository.GetByIdAsync(game.Id, Arg.Any<CancellationToken>()).Returns(game);
            var command = new RecordGameResultCommand(game.Id, "Home");
            var sut = CreateSut();

            var result = await sut.HandleAsync(command, TestContext.Current.CancellationToken);

            Assert.Equal(GameStatusEnum.Finished, result.Status);
            Assert.Equal(GameTeamEnum.Home, result.Winner);
            await GamesRepository.Received(1).UpdateAsync(game, Arg.Any<CancellationToken>());
            await UnitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task ShouldDistributeRatingChange_AcrossPlayersOnATeam()
        {
            // 2v2, evenly rated (1000 each) - a home win splits the +16/-16 team swing evenly:
            // +8 per home player, -8 per away player.
            var game = CreateGame();
            var home1 = AddDummyPlayer(game, GameTeamEnum.Home, 1000);
            var home2 = AddDummyPlayer(game, GameTeamEnum.Home, 1000);
            var away1 = AddDummyPlayer(game, GameTeamEnum.Away, 1000);
            var away2 = AddDummyPlayer(game, GameTeamEnum.Away, 1000);
            GamesRepository.GetByIdAsync(game.Id, Arg.Any<CancellationToken>()).Returns(game);
            var command = new RecordGameResultCommand(game.Id, "Home");
            var sut = CreateSut();

            await sut.HandleAsync(command, TestContext.Current.CancellationToken);

            Assert.Equal(8, home1.RatingChange);
            Assert.Equal(8, home2.RatingChange);
            Assert.Equal(-8, away1.RatingChange);
            Assert.Equal(-8, away2.RatingChange);
            await PlayersRepository.Received(1).UpdateAsync(home1, Arg.Any<CancellationToken>());
            await PlayersRepository.Received(1).UpdateAsync(home2, Arg.Any<CancellationToken>());
            await PlayersRepository.Received(1).UpdateAsync(away1, Arg.Any<CancellationToken>());
            await PlayersRepository.Received(1).UpdateAsync(away2, Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task ShouldNotUpdatePlayer_WhenNotAssignedToATeam()
        {
            var game = CreateGame();
            AddDummyPlayer(game, GameTeamEnum.Home, 1000);
            AddDummyPlayer(game, GameTeamEnum.Away, 1000);
            var unassigned = AddDummyPlayer(game, GameTeamEnum.None, 1000);
            GamesRepository.GetByIdAsync(game.Id, Arg.Any<CancellationToken>()).Returns(game);
            var command = new RecordGameResultCommand(game.Id, "Home");
            var sut = CreateSut();

            await sut.HandleAsync(command, TestContext.Current.CancellationToken);

            Assert.Null(unassigned.RatingChange);
            await PlayersRepository.DidNotReceive().UpdateAsync(unassigned, Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task ShouldApplyRatingChangeToAssociatedUser_WhenPlayerHasUser()
        {
            var game = CreateGame();
            var user = new User("display-name", "external-id", "user@example.com", null);
            var userPlayer = AddUserPlayer(game, GameTeamEnum.Home, user);
            AddDummyPlayer(game, GameTeamEnum.Home, 1000);
            AddDummyPlayer(game, GameTeamEnum.Away, 1000);
            AddDummyPlayer(game, GameTeamEnum.Away, 1000);
            GamesRepository.GetByIdAsync(game.Id, Arg.Any<CancellationToken>()).Returns(game);
            var command = new RecordGameResultCommand(game.Id, "Home");
            var sut = CreateSut();

            await sut.HandleAsync(command, TestContext.Current.CancellationToken);

            Assert.Equal(1008, user.Rating);
            Assert.Equal(userPlayer.RatingChange, user.Rating - 1000);
            await UsersRepository.Received(1).UpdateAsync(user, Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task ShouldNotUpdateAnyUser_WhenAllPlayersAreDummies()
        {
            var game = CreateGame();
            AddDummyPlayer(game, GameTeamEnum.Home, 1000);
            AddDummyPlayer(game, GameTeamEnum.Away, 1000);
            GamesRepository.GetByIdAsync(game.Id, Arg.Any<CancellationToken>()).Returns(game);
            var command = new RecordGameResultCommand(game.Id, "Home");
            var sut = CreateSut();

            await sut.HandleAsync(command, TestContext.Current.CancellationToken);

            await UsersRepository.DidNotReceive().UpdateAsync(Arg.Any<User>(), Arg.Any<CancellationToken>());
        }
    }
}