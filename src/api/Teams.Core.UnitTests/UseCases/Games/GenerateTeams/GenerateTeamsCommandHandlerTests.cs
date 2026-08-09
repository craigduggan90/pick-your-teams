using Teams.Core.Exceptions;
using Teams.Core.UseCases.Games.GenerateTeams;
using Teams.Domain.Entities;
using Teams.Domain.Enums;

namespace Teams.Core.UnitTests.UseCases.Games.GenerateTeams;

public static class GenerateTeamsCommandHandlerTests
{
    public class HandleAsync : UseCaseTestBase<GenerateTeamsCommand>
    {
        private static Game CreateExistingGame() =>
            new("organiser-id", "location", DateTime.UtcNow, 60, 2); // TeamSize 2, MaxPlayers 4

        private static Player AddPlayer(Game game, int rating)
        {
            var player = new Player(game, $"dummy-{Guid.NewGuid():N}", rating);
            player.AssignTeam(GameTeamEnum.None, null);
            game.Players.Add(player);
            return player;
        }

        private GenerateTeamsCommandHandler CreateSut() => new(GamesRepository, Validator);

        [Fact]
        public async Task ShouldThrowRequestHandlerExceptionAndNotLoadGame_WhenValidationFails()
        {
            SetupValidator(InvalidResult());
            var command = new GenerateTeamsCommand("game-id", [], [], 100, 3);
            var sut = CreateSut();

            await Assert.ThrowsAsync<CommandValidationException>(
                () => sut.HandleAsync(command, TestContext.Current.CancellationToken));

            await GamesRepository.DidNotReceive().GetByIdAsync(Arg.Any<string>(), Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task ShouldThrowNotFoundException_WhenGameDoesNotExist()
        {
            GamesRepository.GetByIdAsync("missing-game", Arg.Any<CancellationToken>()).Returns((Game?)null);
            var command = new GenerateTeamsCommand("missing-game", [], [], 100, 3);
            var sut = CreateSut();

            var exception = await Assert.ThrowsAsync<NotFoundException>(
                () => sut.HandleAsync(command, TestContext.Current.CancellationToken));

            Assert.Equal(nameof(Game), exception.ResourceType);
            Assert.Equal("missing-game", exception.ResourceIdentifier);
        }

        [Fact]
        public async Task ShouldThrowRequestHandlerException_WhenGameThrowsTeamGenerationException()
        {
            // Fewer than 2 players triggers TeamGenerationException inside Game.GetTeamSuggestions.
            var game = CreateExistingGame();
            AddPlayer(game, 1000);
            GamesRepository.GetByIdAsync(game.Id, Arg.Any<CancellationToken>()).Returns(game);
            var command = new GenerateTeamsCommand(game.Id, [], [], 100, 3);
            var sut = CreateSut();

            var exception = await Assert.ThrowsAsync<RequestHandlerException>(
                () => sut.HandleAsync(command, TestContext.Current.CancellationToken));
        }

        [Fact]
        public async Task ShouldReturnSuggestionsFromTheLoadedGame_UsingRequestParameters()
        {
            var game = CreateExistingGame();
            var home1 = AddPlayer(game, 1000);
            var home2 = AddPlayer(game, 1000);
            var away1 = AddPlayer(game, 950);
            var away2 = AddPlayer(game, 1050);
            GamesRepository.GetByIdAsync(game.Id, Arg.Any<CancellationToken>()).Returns(game);
            var command = new GenerateTeamsCommand(game.Id, [home1.Id, home2.Id], [away1.Id, away2.Id], 100, 3);
            var sut = CreateSut();

            var result = await sut.HandleAsync(command, TestContext.Current.CancellationToken);

            var suggestion = Assert.Single(result);
            Assert.Equal([home1, home2], suggestion.Home);
            Assert.Equal([away1, away2], suggestion.Away);
            Assert.Equal(0, suggestion.TeamDifferential);
        }

        [Fact]
        public async Task ShouldReturnEmpty_WhenNoSuggestionMeetsTheRequestedDifferential()
        {
            var game = CreateExistingGame();
            var home1 = AddPlayer(game, 1500);
            var home2 = AddPlayer(game, 1500);
            var away1 = AddPlayer(game, 500);
            var away2 = AddPlayer(game, 500);
            GamesRepository.GetByIdAsync(game.Id, Arg.Any<CancellationToken>()).Returns(game);
            var command = new GenerateTeamsCommand(game.Id, [home1.Id, home2.Id], [away1.Id, away2.Id], 100, 3);
            var sut = CreateSut();

            var result = await sut.HandleAsync(command, TestContext.Current.CancellationToken);

            Assert.Empty(result);
        }
    }
}