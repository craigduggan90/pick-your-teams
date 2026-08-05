using Teams.Domain.Entities;
using Teams.Domain.Enums;
using Teams.Domain.Exceptions;
using Teams.Domain.UnitTests.TestHelpers;

namespace Teams.Domain.UnitTests.Entities;

public static partial class GameTests
{
    public abstract class GameTestsBase
    {
        protected const string DefaultLocation = "The Arena";
        protected static readonly DateTime DefaultStartTime = new(2026, 3, 1, 18, 0, 0, DateTimeKind.Utc);
        protected const int DefaultDuration = 60;
        protected const int DefaultTeamSize = 2;
        protected const string DefaultOrganiserId = "organiser-001";

        protected static Game CreateGame(Action<Game>? setup = null)
        {
            var game = new Game(DefaultOrganiserId, DefaultLocation, DefaultStartTime, DefaultDuration, DefaultTeamSize);
            setup?.Invoke(game);
            return game;
        }

        protected static User CreateUser() => new("display-name", "external-id", "user@example.com", null);

        protected static Player AddPlayer(Game game, GameTeamEnum team, int rating)
        {
            var player = new Player(game, $"player-{Guid.NewGuid():N}", rating);
            player.AssignTeam(team, null);
            game.Players.Add(player);
            return player;
        }
    }

    public class Constructor : GameTestsBase
    {
        [Fact]
        public void CreatesGame_FromParameters()
        {
            var game = CreateGame();

            Assert.Equal(DefaultLocation, game.Location);
            Assert.Equal(DefaultStartTime, game.StartTime);
            Assert.Equal(DefaultDuration, game.Duration);
            Assert.Equal(DefaultTeamSize, game.TeamSize);
            Assert.Equal(DefaultOrganiserId, game.OrganiserId);
            Assert.Equal(GameStatusEnum.Scheduled, game.Status);
            Assert.Null(game.HomeTeamRating);
            Assert.Null(game.AwayTeamRating);
            Assert.Null(game.Winner);
            Assert.Empty(game.Players);
        }

        [Fact]
        public void AllowsNullLocation()
        {
            var game = new Game(DefaultOrganiserId, null, DefaultStartTime, DefaultDuration, DefaultTeamSize);

            Assert.Null(game.Location);
        }
    }

    public class MaxPlayers
    {
        [Theory]
        [InlineData(1, 2)]
        [InlineData(5, 10)]
        [InlineData(11, 22)]
        public void ReturnsDoubleTeamSize(int teamSize, int expected)
        {
            var game = new Game("organiser-001", "location", DateTime.UtcNow, 60, teamSize);

            Assert.Equal(expected, game.MaxPlayers);
        }
    }

    public class Update : GameTestsBase
    {
        [Fact]
        public void UpdatesLocationStartTimeAndDuration_WhenAllProvided()
        {
            var game = CreateGame();

            game.Update("New Venue", DefaultStartTime.AddDays(1), 90);

            Assert.Equal("New Venue", game.Location);
            Assert.Equal(DefaultStartTime.AddDays(1), game.StartTime);
            Assert.Equal(90, game.Duration);
        }

        [Fact]
        public void LeavesValuesUnchanged_WhenArgumentsAreNull()
        {
            var game = CreateGame();

            game.Update(null, null, null);

            Assert.Equal(DefaultLocation, game.Location);
            Assert.Equal(DefaultStartTime, game.StartTime);
            Assert.Equal(DefaultDuration, game.Duration);
        }

        [Fact]
        public void UpdatesOnlyProvidedFields_WhenPartiallySpecified()
        {
            var game = CreateGame();

            game.Update("New Venue", null, null);

            Assert.Equal("New Venue", game.Location);
            Assert.Equal(DefaultStartTime, game.StartTime);
            Assert.Equal(DefaultDuration, game.Duration);
        }
    }

    public class OrganiserProperty : GameTestsBase
    {
        [Fact]
        public void ThrowsUninitializedPropertyException_WhenOrganiserNotSet()
        {
            var game = CreateGame();

            Assert.Throws<UninitializedPropertyException>(() => game.Organiser);
        }

        [Fact]
        public void ReturnsAssignedOrganiser_WhenSetViaObjectInitializer()
        {
            var organiser = CreateUser();
            var game = new Game(DefaultOrganiserId, DefaultLocation, DefaultStartTime, DefaultDuration, DefaultTeamSize)
            {
                Organiser = organiser
            };

            Assert.Same(organiser, game.Organiser);
        }
    }

    public class SetResult : GameTestsBase
    {
        [Fact]
        public void FinishesGame_AndRecordsWinnerAndTeamRatingSums()
        {
            var game = CreateGame();
            AddPlayer(game, GameTeamEnum.Home, 1000);
            AddPlayer(game, GameTeamEnum.Home, 1000);
            AddPlayer(game, GameTeamEnum.Away, 1000);
            AddPlayer(game, GameTeamEnum.Away, 1000);

            game.SetResult(GameTeamEnum.Home);

            Assert.Equal(GameStatusEnum.Finished, game.Status);
            Assert.Equal(GameTeamEnum.Home, game.Winner);
            Assert.Equal(2000, game.HomeTeamRating);
            Assert.Equal(2000, game.AwayTeamRating);
            Assert.True(game.IsDirty);
        }

        [Fact]
        public void IgnoresPlayersWithNoTeamAssigned()
        {
            var game = CreateGame();
            AddPlayer(game, GameTeamEnum.Home, 1000);
            AddPlayer(game, GameTeamEnum.None, 9999);

            game.SetResult(GameTeamEnum.Home);

            Assert.Equal(1000, game.HomeTeamRating);
            Assert.Equal(0, game.AwayTeamRating);
        }

        [Fact]
        public void DoesNothing_WhenGameAlreadyFinished()
        {
            var game = CreateGame();
            AddPlayer(game, GameTeamEnum.Home, 1000);
            AddPlayer(game, GameTeamEnum.Away, 1000);
            game.SetResult(GameTeamEnum.Home);
            var winnerAfterFirstCall = game.Winner;
            var homeRatingAfterFirstCall = game.HomeTeamRating;

            AddPlayer(game, GameTeamEnum.Home, 5000);
            game.SetResult(GameTeamEnum.Away);

            Assert.Equal(winnerAfterFirstCall, game.Winner);
            Assert.Equal(homeRatingAfterFirstCall, game.HomeTeamRating);
        }
    }

    public class HomeTeamRatingChange : GameTestsBase
    {
        [Fact]
        public void ReturnsNull_WhenGameNotYetFinished()
        {
            var game = CreateGame();

            Assert.Null(game.HomeTeamRatingChange);
        }

        [Fact]
        public void ReturnsPositiveChange_WhenHomeTeamWinsEvenlyMatchedGame()
        {
            var game = CreateGame();
            AddPlayer(game, GameTeamEnum.Home, 1000);
            AddPlayer(game, GameTeamEnum.Home, 1000);
            AddPlayer(game, GameTeamEnum.Away, 1000);
            AddPlayer(game, GameTeamEnum.Away, 1000);

            game.SetResult(GameTeamEnum.Home);

            Assert.Equal(16.0, game.HomeTeamRatingChange!.Value, precision: 4);
        }

        [Fact]
        public void ReturnsLargeNegativeChange_WhenUnderdogAwayTeamWins()
        {
            var game = CreateGame();
            AddPlayer(game, GameTeamEnum.Home, 1100);
            AddPlayer(game, GameTeamEnum.Home, 1100);
            AddPlayer(game, GameTeamEnum.Away, 900);
            AddPlayer(game, GameTeamEnum.Away, 900);

            game.SetResult(GameTeamEnum.Away);

            Assert.Equal(-29.090909, game.HomeTeamRatingChange!.Value, precision: 4);
        }

        [Fact]
        public void ReturnsNegativeChange_WhenHomeTeamWasFavouredAndGameDraws()
        {
            var game = CreateGame();
            AddPlayer(game, GameTeamEnum.Home, 1050);
            AddPlayer(game, GameTeamEnum.Home, 1050);
            AddPlayer(game, GameTeamEnum.Away, 950);
            AddPlayer(game, GameTeamEnum.Away, 950);

            game.SetResult(GameTeamEnum.None);

            Assert.Equal(-8.311902, game.HomeTeamRatingChange!.Value, precision: 4);
        }

        [Fact]
        public void ReturnsSameValue_OnRepeatedAccess()
        {
            var game = CreateGame();
            AddPlayer(game, GameTeamEnum.Home, 1000);
            AddPlayer(game, GameTeamEnum.Away, 1000);
            game.SetResult(GameTeamEnum.Home);

            var first = game.HomeTeamRatingChange;
            var second = game.HomeTeamRatingChange;

            Assert.Equal(first, second);
        }
    }

    public class AwayTeamRatingChange : GameTestsBase
    {
        [Fact]
        public void ReturnsNull_WhenGameNotYetFinished()
        {
            var game = CreateGame();

            Assert.Null(game.AwayTeamRatingChange);
        }

        [Fact]
        public void ReturnsNegativeChange_WhenHomeTeamWinsEvenlyMatchedGame()
        {
            var game = CreateGame();
            AddPlayer(game, GameTeamEnum.Home, 1000);
            AddPlayer(game, GameTeamEnum.Home, 1000);
            AddPlayer(game, GameTeamEnum.Away, 1000);
            AddPlayer(game, GameTeamEnum.Away, 1000);

            game.SetResult(GameTeamEnum.Home);

            Assert.Equal(-16.0, game.AwayTeamRatingChange!.Value, precision: 4);
        }

        [Fact]
        public void ReturnsLargePositiveChange_WhenUnderdogAwayTeamWins()
        {
            var game = CreateGame();
            AddPlayer(game, GameTeamEnum.Home, 1100);
            AddPlayer(game, GameTeamEnum.Home, 1100);
            AddPlayer(game, GameTeamEnum.Away, 900);
            AddPlayer(game, GameTeamEnum.Away, 900);

            game.SetResult(GameTeamEnum.Away);

            Assert.Equal(29.090909, game.AwayTeamRatingChange!.Value, precision: 4);
        }

        [Fact]
        public void ReturnsPositiveChange_WhenAwayTeamWasUnderdogAndGameDraws()
        {
            var game = CreateGame();
            AddPlayer(game, GameTeamEnum.Home, 1050);
            AddPlayer(game, GameTeamEnum.Home, 1050);
            AddPlayer(game, GameTeamEnum.Away, 950);
            AddPlayer(game, GameTeamEnum.Away, 950);

            game.SetResult(GameTeamEnum.None);

            Assert.Equal(8.311902, game.AwayTeamRatingChange!.Value, precision: 4);
        }
    }

    public class HomeTeamPlayerCount : GameTestsBase
    {
        [Fact]
        public void ReturnsZero_WhenNoPlayersAssignedToHomeTeam()
        {
            var game = CreateGame();

            Assert.Equal(0, game.HomeTeamPlayerCount);
        }

        [Fact]
        public void CountsOnlyHomeTeamPlayers()
        {
            var game = CreateGame();
            AddPlayer(game, GameTeamEnum.Home, 1000);
            AddPlayer(game, GameTeamEnum.Home, 1100);
            AddPlayer(game, GameTeamEnum.Away, 1000);
            AddPlayer(game, GameTeamEnum.None, 1000);

            Assert.Equal(2, game.HomeTeamPlayerCount);
        }
    }

    public class AwayTeamPlayerCount : GameTestsBase
    {
        [Fact]
        public void ReturnsZero_WhenNoPlayersAssignedToAwayTeam()
        {
            var game = CreateGame();

            Assert.Equal(0, game.AwayTeamPlayerCount);
        }

        [Fact]
        public void CountsOnlyAwayTeamPlayers()
        {
            var game = CreateGame();
            AddPlayer(game, GameTeamEnum.Away, 1000);
            AddPlayer(game, GameTeamEnum.Away, 1100);
            AddPlayer(game, GameTeamEnum.Home, 1000);
            AddPlayer(game, GameTeamEnum.None, 1000);

            Assert.Equal(2, game.AwayTeamPlayerCount);
        }
    }

    public class AsSerializable : GameTestsBase
    {
        [Fact]
        public void IncludesIdAndTimestamps_WhenCalled()
        {
            var game = CreateGame();

            var serializable = game.AsSerializable();
            var type = serializable.GetType();

            Assert.Equal(game.Id, serializable.GetValue(type, "Id"));
            Assert.Equal(game.DateCreated, serializable.GetValue(type, "DateCreated"));
            Assert.Equal(game.DateModified, serializable.GetValue(type, "DateModified"));
        }

        [Fact]
        public void ExcludesLocationStatusOrganiserAndTeamRatings_WhenCalled()
        {
            var game = CreateGame();

            var serializable = game.AsSerializable();
            var type = serializable.GetType();

            Assert.Null(serializable.GetValue(type, "Location"));
            Assert.Null(serializable.GetValue(type, "StartTime"));
            Assert.Null(serializable.GetValue(type, "Duration"));
            Assert.Null(serializable.GetValue(type, "OrganiserId"));
            Assert.Null(serializable.GetValue(type, "Status"));
            Assert.Null(serializable.GetValue(type, "HomeTeamRating"));
            Assert.Null(serializable.GetValue(type, "AwayTeamRating"));
            Assert.Null(serializable.GetValue(type, "Winner"));
        }
    }
}