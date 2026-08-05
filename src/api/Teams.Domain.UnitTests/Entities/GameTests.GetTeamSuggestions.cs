using Teams.Domain.Entities;
using Teams.Domain.Enums;
using Teams.Domain.Exceptions;

namespace Teams.Domain.UnitTests.Entities;

public static partial class GameTests
{
    public class GetTeamSuggestions : GameTestsBase
    {
        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        [InlineData(6)]
        public void ShouldThrow_WhenMaxSuggestionsIsOutOfRange(int maxSuggestions)
        {
            var game = CreateGame();
            AddPlayer(game, GameTeamEnum.None, 1000);
            AddPlayer(game, GameTeamEnum.None, 1000);

            Assert.Throws<TeamGenerationException>(
                () => game.GetTeamSuggestions([], [], 100, maxSuggestions));
        }

        [Theory]
        [InlineData(0)]
        [InlineData(1)]
        public void ShouldThrow_WhenFewerThanTwoPlayersAdded(int playerCount)
        {
            var game = CreateGame();
            for (var i = 0; i < playerCount; i++)
                AddPlayer(game, GameTeamEnum.None, 1000);

            Assert.Throws<TeamGenerationException>(
                () => game.GetTeamSuggestions([], [], 100, 3));
        }

        [Fact]
        public void ShouldThrow_WhenMorePlayersThanMaxPlayersAdded()
        {
            var game = CreateGame(); // TeamSize = 2, MaxPlayers = 4
            for (var i = 0; i < 5; i++)
                AddPlayer(game, GameTeamEnum.None, 1000);

            Assert.Throws<TeamGenerationException>(
                () => game.GetTeamSuggestions([], [], 100, 3));
        }

        [Fact]
        public void ShouldThrow_WhenHomeSeedExceedsTeamSize()
        {
            var game = CreateGame(); // TeamSize = 2
            var players = Enumerable.Range(0, 4).Select(_ => AddPlayer(game, GameTeamEnum.None, 1000)).ToArray();
            var homeSeedIds = players.Take(3).Select(p => p.Id).ToArray();

            var exception = Assert.Throws<TeamGenerationException>(
                () => game.GetTeamSuggestions(homeSeedIds, [], 100, 3));
            Assert.Contains("home", exception.Message);
        }

        [Fact]
        public void ShouldThrow_WhenAwaySeedExceedsTeamSize()
        {
            var game = CreateGame(); // TeamSize = 2
            var players = Enumerable.Range(0, 4).Select(_ => AddPlayer(game, GameTeamEnum.None, 1000)).ToArray();
            var awaySeedIds = players.Take(3).Select(p => p.Id).ToArray();

            var exception = Assert.Throws<TeamGenerationException>(
                () => game.GetTeamSuggestions([], awaySeedIds, 100, 3));
            Assert.Contains("away", exception.Message);
        }

        [Fact]
        public void ShouldThrow_WhenHomeSeedLeavesTooFewUnassignedPlayersForHomeSize()
        {
            // TeamSize = 5, so the per-side cap (5) doesn't trip first - but only 4 players exist so
            // far, so the *current* home size is (4+1)/2 = 2. Seeding 3 to home is already too many
            // for this partial roster, even though 3 <= TeamSize.
            var game = new Game(DefaultOrganiserId, DefaultLocation, DefaultStartTime, DefaultDuration, 5);
            var players = Enumerable.Range(0, 4).Select(_ => AddPlayer(game, GameTeamEnum.None, 1000)).ToArray();
            var homeSeedIds = players.Take(3).Select(p => p.Id).ToArray();

            var exception = Assert.Throws<TeamGenerationException>(
                () => game.GetTeamSuggestions(homeSeedIds, [], 100, 3));
            Assert.Contains("home", exception.Message);
        }

        [Fact]
        public void ShouldThrow_WhenAwaySeedLeavesTooFewUnassignedPlayersForHomeSize()
        {
            // Mirror of the above: 0 seeded home, 3 seeded away out of 4 total players. Home size is
            // still 2, but only 1 player is left unassigned to fill it.
            var game = new Game(DefaultOrganiserId, DefaultLocation, DefaultStartTime, DefaultDuration, 5);
            var players = Enumerable.Range(0, 4).Select(_ => AddPlayer(game, GameTeamEnum.None, 1000)).ToArray();
            var awaySeedIds = players.Take(3).Select(p => p.Id).ToArray();

            var exception = Assert.Throws<TeamGenerationException>(
                () => game.GetTeamSuggestions([], awaySeedIds, 100, 3));
            Assert.Contains("away", exception.Message);
        }

        [Fact]
        public void ShouldReturnPreSeededSuggestion_WhenAllPlayersSeededAndWithinThreshold()
        {
            var game = CreateGame(); // TeamSize = 2
            var home1 = AddPlayer(game, GameTeamEnum.None, 1000);
            var home2 = AddPlayer(game, GameTeamEnum.None, 1000);
            var away1 = AddPlayer(game, GameTeamEnum.None, 950);
            var away2 = AddPlayer(game, GameTeamEnum.None, 1050);

            var result = game.GetTeamSuggestions([home1.Id, home2.Id], [away1.Id, away2.Id], 100, 3);

            var suggestion = Assert.Single(result);
            Assert.Equal([home1, home2], suggestion.Home);
            Assert.Equal([away1, away2], suggestion.Away);
            Assert.Equal(2000, suggestion.HomeRating);
            Assert.Equal(2000, suggestion.AwayRating);
            Assert.Equal(0, suggestion.TeamDifferential);
        }

        [Fact]
        public void ShouldReturnEmpty_WhenAllPlayersSeededButExceedsThreshold()
        {
            var game = CreateGame(); // TeamSize = 2
            var home1 = AddPlayer(game, GameTeamEnum.None, 1500);
            var home2 = AddPlayer(game, GameTeamEnum.None, 1500);
            var away1 = AddPlayer(game, GameTeamEnum.None, 500);
            var away2 = AddPlayer(game, GameTeamEnum.None, 500);

            var result = game.GetTeamSuggestions([home1.Id, home2.Id], [away1.Id, away2.Id], 100, 3);

            Assert.Empty(result);
        }

        [Fact]
        public void ShouldGenerateSuggestions_WithinThreshold_WhenNoPlayersAreSeeded()
        {
            var game = CreateGame(); // TeamSize = 2, MaxPlayers = 4
            AddPlayer(game, GameTeamEnum.None, 900);
            AddPlayer(game, GameTeamEnum.None, 1100);
            AddPlayer(game, GameTeamEnum.None, 1000);
            AddPlayer(game, GameTeamEnum.None, 1000);

            var result = game.GetTeamSuggestions([], [], 500, 3);

            Assert.NotEmpty(result);
            foreach (var suggestion in result)
            {
                Assert.Equal(2, suggestion.Home.Count);
                Assert.Equal(2, suggestion.Away.Count);
                Assert.Equal(suggestion.Home.Sum(p => p.Rating), suggestion.HomeRating);
                Assert.Equal(suggestion.Away.Sum(p => p.Rating), suggestion.AwayRating);
                Assert.Equal(Math.Abs(suggestion.HomeRating - suggestion.AwayRating), suggestion.TeamDifferential);
                Assert.True(suggestion.TeamDifferential <= 500);
            }
        }

        [Fact]
        public void ShouldRespectMaxSuggestions_WhenMoreCombinationsMatchThanRequested()
        {
            var game = new Game(DefaultOrganiserId, DefaultLocation, DefaultStartTime, DefaultDuration, 3); // MaxPlayers = 6
            for (var i = 0; i < 6; i++)
                AddPlayer(game, GameTeamEnum.None, 1000); // every split is perfectly even

            var result = game.GetTeamSuggestions([], [], 0, 5);

            Assert.Equal(5, result.Count);
        }

        [Fact]
        public void ShouldGiveHomeTheExtraPlayer_WhenTotalPlayerCountIsOdd()
        {
            var game = new Game(DefaultOrganiserId, DefaultLocation, DefaultStartTime, DefaultDuration, 3); // MaxPlayers = 6
            for (var i = 0; i < 3; i++)
                AddPlayer(game, GameTeamEnum.None, 1000);

            var result = game.GetTeamSuggestions([], [], 100000, 5);

            Assert.NotEmpty(result);
            Assert.All(result, suggestion =>
            {
                Assert.Equal(2, suggestion.Home.Count);
                Assert.Single(suggestion.Away);
            });
        }

        [Fact]
        public void ShouldKeepSeededPlayersOnTheirAssignedTeam_InEverySuggestion()
        {
            var game = new Game(DefaultOrganiserId, DefaultLocation, DefaultStartTime, DefaultDuration, 3); // MaxPlayers = 6
            var seededHome = AddPlayer(game, GameTeamEnum.None, 1000);
            for (var i = 0; i < 5; i++)
                AddPlayer(game, GameTeamEnum.None, 1000);

            var result = game.GetTeamSuggestions([seededHome.Id], [], 100000, 5);

            Assert.NotEmpty(result);
            Assert.All(result, suggestion =>
            {
                Assert.Contains(seededHome, suggestion.Home);
                Assert.DoesNotContain(seededHome, suggestion.Away);
            });
        }

        [Fact]
        public void ShouldReturnSingleSuggestion_WhenHomeSeededToFullSizeAndAwayUnseeded()
        {
            var game = CreateGame(); // TeamSize = 2
            var home1 = AddPlayer(game, GameTeamEnum.None, 1000);
            var home2 = AddPlayer(game, GameTeamEnum.None, 1000);
            var away1 = AddPlayer(game, GameTeamEnum.None, 950);
            var away2 = AddPlayer(game, GameTeamEnum.None, 1050);

            var result = game.GetTeamSuggestions([home1.Id, home2.Id], [], 100, 5);

            var suggestion = Assert.Single(result);
            Assert.Equal([home1, home2], suggestion.Home);
            Assert.Equal([away1, away2], suggestion.Away);
        }
        
        [Fact]
        public void ShouldExcludeCombinations_ThatExceedTheDifferentialThreshold()
        {
            // 4 players (TeamSize 2): ratings 500, 1500, 1000, 1000.
            // Home={500,1500} vs Away={1000,1000}: 2000 vs 2000, differential 0 - passes.
            // Home={500,1000} vs Away={1500,1000}: 1500 vs 2500, differential 1000 - excluded.
            var game = CreateGame();
            AddPlayer(game, GameTeamEnum.None, 500);
            AddPlayer(game, GameTeamEnum.None, 1500);
            AddPlayer(game, GameTeamEnum.None, 1000);
            AddPlayer(game, GameTeamEnum.None, 1000);

            var result = game.GetTeamSuggestions([], [], 0, 5);

            Assert.All(result, suggestion => Assert.Equal(0, suggestion.TeamDifferential));
        }
    }
}