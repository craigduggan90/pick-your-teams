using Teams.Domain.Entities;
using Teams.Domain.Enums;
using Teams.Domain.Exceptions;
using Teams.Domain.UnitTests.TestHelpers;

namespace Teams.Domain.UnitTests.Entities;

public static class PlayerTests
{
    public abstract class PlayerTestsBase
    {
        protected const string DefaultGameId = "game-001";
        protected const string DefaultUserId = "user-001";
        protected const string DefaultDisplayName = "display-name";
        protected const int DefaultRating = 1200;
        protected const PlayerTypeEnum DefaultType = PlayerTypeEnum.User;
        protected const GameTeamEnum DefaultTeam = GameTeamEnum.Home;

        protected static Player CreatePlayer(Action<Player>? setup = null)
        {
            var player = new Player(DefaultGameId, DefaultUserId, DefaultRating, DefaultType, DefaultTeam)
            {
                DisplayName = DefaultDisplayName
            };
            setup?.Invoke(player);
            return player;
        }

        protected static Game CreateGame() =>
            new("organiser-id", "location", DateTime.UtcNow, 60, 5);

        protected static User CreateUser() =>
            new("display name", "external-id", "email@example.com", null);

        protected static Player CreatePlayerWithRating(int rating) =>
            new(DefaultGameId, DefaultUserId, rating, DefaultType, DefaultTeam) { DisplayName = DefaultDisplayName };
    }

    public class Constructor : PlayerTestsBase
    {
        [Fact]
        public void CreatesPlayer_FromExplicitValues()
        {
            var player = CreatePlayer();

            Assert.Equal(DefaultGameId, player.GameId);
            Assert.Equal(DefaultUserId, player.UserId);
            Assert.Equal(DefaultDisplayName, player.GetDisplayName());
            Assert.Equal(DefaultRating, player.Rating);
            Assert.Equal(DefaultType, player.Type);
            Assert.Equal(DefaultTeam, player.Team);
            Assert.Null(player.RatingChange);
            Assert.Null(player.User);
        }

        [Fact]
        public void CreatesDummyPlayer_FromParameters()
        {
            var game = CreateGame();

            var player = new Player(game, "dummy-name", 950);

            Assert.Equal(game.Id, player.GameId);
            Assert.Same(game, player.Game);
            Assert.Equal("dummy-name", player.GetDisplayName());
            Assert.Equal(950, player.Rating);
            Assert.Null(player.UserId);
            Assert.Null(player.User);
            Assert.Equal(PlayerTypeEnum.Dummy, player.Type);
            Assert.Equal(GameTeamEnum.None, player.Team);
        }

        [Fact]
        public void CreatesUserPlayer_FromRelations()
        {
            var game = CreateGame();
            var user = CreateUser();

            var player = new Player(game, user);

            Assert.Equal(game.Id, player.GameId);
            Assert.Same(game, player.Game);
            Assert.Equal(user.Id, player.UserId);
            Assert.Same(user, player.User);
            Assert.Null(player.DisplayName); // no snapshot taken - GetDisplayName() falls back to User.DisplayName
            Assert.Equal(user.DisplayName, player.GetDisplayName());
            Assert.Equal(user.Rating, player.Rating);
            Assert.Equal(PlayerTypeEnum.User, player.Type);
            Assert.Equal(GameTeamEnum.None, player.Team);
        }
    }

    public class GetDisplayName : PlayerTestsBase
    {
        [Fact]
        public void ReturnsDisplayName_WhenUserIsNull()
        {
            var player = new Player(DefaultGameId, null, DefaultRating, PlayerTypeEnum.Dummy, DefaultTeam)
            {
                DisplayName = "Dummy Player"
            };

            Assert.Equal("Dummy Player", player.GetDisplayName());
        }

        [Fact]
        public void ReturnsUserDisplayName_WhenUserIsSet()
        {
            var user = CreateUser();
            var player = new Player(DefaultGameId, user.Id, DefaultRating, PlayerTypeEnum.User, DefaultTeam)
            {
                User = user
            };

            Assert.Equal(user.DisplayName, player.GetDisplayName());
        }

        [Fact]
        public void PrefersUserDisplayName_WhenBothUserAndDisplayNameAreSet()
        {
            // Guards against the fallback order silently flipping - a stale snapshot must never win over the
            // linked user's current, live display name.
            var user = CreateUser();
            var player = new Player(DefaultGameId, user.Id, DefaultRating, PlayerTypeEnum.User, DefaultTeam)
            {
                User = user,
                DisplayName = "Stale Snapshot"
            };

            Assert.Equal(user.DisplayName, player.GetDisplayName());
        }

        [Fact]
        public void ReturnsNull_WhenNeitherUserNorDisplayNameIsSet()
        {
            var player = new Player(DefaultGameId, null, DefaultRating, PlayerTypeEnum.Dummy, DefaultTeam);

            Assert.Null(player.GetDisplayName());
        }
    }

    public class AssignTeam : PlayerTestsBase
    {
        [Fact]
        public void AssignsTeamAndFixesRating_WhenRatingProvided()
        {
            var player = CreatePlayer();

            player.AssignTeam(GameTeamEnum.Away, 1350);

            Assert.Equal(GameTeamEnum.Away, player.Team);
            Assert.Equal(1350, player.Rating);
            Assert.True(player.IsDirty);
        }

        [Fact]
        public void AssignsTeamOnly_WhenRatingIsNull()
        {
            var player = CreatePlayer();

            player.AssignTeam(GameTeamEnum.Away, null);

            Assert.Equal(GameTeamEnum.Away, player.Team);
            Assert.Equal(DefaultRating, player.Rating);
        }
    }

    public class SetRatingChange : PlayerTestsBase
    {
        [Fact]
        public void GivesLowerRatedPlayers_ALargerShareOfAPositiveChange()
        {
            var lowerRated = CreatePlayerWithRating(1000);
            var higherRated = CreatePlayerWithRating(1200);

            lowerRated.SetRatingChange(teamRating: 2200, teamChange: 16, teamSize: 2);
            higherRated.SetRatingChange(teamRating: 2200, teamChange: 16, teamSize: 2);

            Assert.Equal(9, lowerRated.RatingChange);
            Assert.Equal(7, higherRated.RatingChange);
        }

        [Fact]
        public void GivesHigherRatedPlayers_ALargerShareOfANegativeChange()
        {
            var lowerRated = CreatePlayerWithRating(900);
            var higherRated = CreatePlayerWithRating(1300);

            lowerRated.SetRatingChange(teamRating: 2200, teamChange: -16, teamSize: 2);
            higherRated.SetRatingChange(teamRating: 2200, teamChange: -16, teamSize: 2);

            Assert.Equal(-7, lowerRated.RatingChange);
            Assert.Equal(-9, higherRated.RatingChange);
        }

        [Fact]
        public void DoesNotUseTeamSize_WhenDistributingANegativeChange()
        {
            // Guards against the loss branch accidentally picking up the win branch's
            // (teamSize - 1) denominator - if it did, only half of a 3-player team's loss would
            // get distributed instead of the full teamChange.
            var players = new[] { 700, 800, 900 }.Select(CreatePlayerWithRating).ToArray();

            foreach (var player in players)
                player.SetRatingChange(teamRating: 2400, teamChange: -24, teamSize: 3);

            Assert.Equal(-7, players[0].RatingChange);
            Assert.Equal(-8, players[1].RatingChange);
            Assert.Equal(-9, players[2].RatingChange);
        }

        [Fact]
        public void DoesNotAlterRating()
        {
            var player = CreatePlayerWithRating(1000);

            player.SetRatingChange(teamRating: 2200, teamChange: 16, teamSize: 2);

            Assert.Equal(1000, player.Rating);
        }
    }

    public class GameProperty : PlayerTestsBase
    {
        [Fact]
        public void ThrowsUninitializedPropertyException_WhenGameNotSet()
        {
            var player = CreatePlayer();

            Assert.Throws<UninitializedPropertyException>(() => player.Game);
        }

        [Fact]
        public void ReturnsAssignedGame_WhenSetViaObjectInitializer()
        {
            var game = CreateGame();
            var player = new Player(DefaultGameId, DefaultUserId, DefaultRating, DefaultType, DefaultTeam)
            {
                Game = game
            };

            Assert.Same(game, player.Game);
        }
    }

    public class AsSerializable : PlayerTestsBase
    {
        [Fact]
        public void IncludesIdGameIdUserIdTeamAndTimestamps_WhenCalled()
        {
            var player = CreatePlayer();

            var serializable = player.AsSerializable();
            var type = serializable.GetType();

            Assert.Equal(player.Id, serializable.GetValue(type, "Id"));
            Assert.Equal(player.GameId, serializable.GetValue(type, "GameId"));
            Assert.Equal(player.UserId, serializable.GetValue(type, "UserId"));
            Assert.Equal(player.Team, serializable.GetValue(type, "Team"));
            Assert.Equal(player.DateCreated, serializable.GetValue(type, "DateCreated"));
            Assert.Equal(player.DateModified, serializable.GetValue(type, "DateModified"));
        }

        [Fact]
        public void ExcludesDisplayNameRatingAndType_WhenCalled()
        {
            var player = CreatePlayer();

            var serializable = player.AsSerializable();
            var type = serializable.GetType();

            Assert.Null(serializable.GetValue(type, "DisplayName"));
            Assert.Null(serializable.GetValue(type, "Rating"));
            Assert.Null(serializable.GetValue(type, "Type"));
        }
    }

    public class UnassignTeam : PlayerTestsBase
    {
        [Fact]
        public void SetsTeamToNone_WhenCalled()
        {
            var player = CreatePlayer(p => p.AssignTeam(GameTeamEnum.Home, null));

            player.UnassignTeam();

            Assert.Equal(GameTeamEnum.None, player.Team);
        }

        [Fact]
        public void DoesNotAlterRating()
        {
            var player = CreatePlayer(p => p.AssignTeam(GameTeamEnum.Home, 1350));

            player.UnassignTeam();

            Assert.Equal(1350, player.Rating);
        }

        [Fact]
        public void LeavesTeamAsNone_WhenAlreadyUnassigned()
        {
            var player = CreatePlayer(p => p.AssignTeam(GameTeamEnum.None, null));

            player.UnassignTeam();

            Assert.Equal(GameTeamEnum.None, player.Team);
        }
    }
}