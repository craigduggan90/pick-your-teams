using Teams.Api.Controllers.V1.Players;
using Teams.Api.Controllers.V1.Players.RequestModels;
using Teams.Common.Pagination;
using Teams.Common.Providers.Identifiers;
using Teams.Domain.Entities;
using Teams.Domain.Enums;

namespace Teams.Api.UnitTests.Controllers.V1.Players;

public static class PlayersMapperTests
{
    private static Player GetPlayer(
        string? id = null,
        string gameId = "test-game-id",
        string? userId = null,
        string displayName = "Test Player",
        int rating = 1000,
        PlayerTypeEnum type = PlayerTypeEnum.Dummy,
        GameTeamEnum team = GameTeamEnum.None)
    {
        using var idFix = new IdentifierProviderContext(id ?? Guid.NewGuid().ToString("N"));
        return new Player(gameId, userId, displayName, rating, type, team);
    }

    public class ToPlayerModel
    {
        [Fact]
        public void MapsAllProperties_WhenCalled()
        {
            var player = GetPlayer(
                userId: "test-user-id",
                displayName: "Marcus Aurelius",
                rating: 900,
                type: PlayerTypeEnum.User,
                team: GameTeamEnum.Home);

            var result = player.ToPlayerModel();

            Assert.Equal(player.Id, result.Id);
            Assert.Equal(player.GameId, result.GameId);
            Assert.Equal(player.UserId, result.UserId);
            Assert.Equal(nameof(PlayerTypeEnum.User), result.Type);
            Assert.Equal(player.GetDisplayName, result.DisplayName);
            Assert.Equal(player.Rating, result.Rating);
            Assert.Equal(nameof(GameTeamEnum.Home), result.Team);
        }
    }

    public class ToPlayerDetailModel
    {
        [Fact]
        public void MapsAllProperties_WhenCalled()
        {
            var player = GetPlayer(
                userId: "test-user-id",
                displayName: "Marcus Aurelius",
                rating: 900,
                type: PlayerTypeEnum.User,
                team: GameTeamEnum.Home);

            var result = player.ToPlayerDetailModel();

            Assert.Equal(player.Id, result.Id);
            Assert.Equal(player.GameId, result.GameId);
            Assert.Equal(player.UserId, result.UserId);
            Assert.Equal(nameof(PlayerTypeEnum.User), result.Type);
            Assert.Equal(player.GetDisplayName, result.DisplayName);
            Assert.Equal(player.Rating, result.Rating);
            Assert.Equal(player.RatingChange, result.RatingChange);
            Assert.Equal(nameof(GameTeamEnum.Home), result.Team);
            Assert.Equal(player.DateCreated, result.Created);
            Assert.Equal(player.DateModified, result.Modified);
        }

        [Fact]
        public void SetsRatingChangeToNull_WhenNotYetCalculated()
        {
            var player = GetPlayer();

            var result = player.ToPlayerDetailModel();

            Assert.Null(result.RatingChange);
        }
    }

    public class ToCommandFromCreatePlayerRequestModel
    {
        [Fact]
        public void MapsAllProperties_WhenCalled()
        {
            var model = new CreatePlayerRequestModel("test-game-id", "test-user-id");

            var result = model.ToCommand();

            Assert.Equal(model.GameId, result.GameId);
            Assert.Equal(model.UserId, result.UserId);
        }
    }

    public class ToCommandFromCreateDummyPlayerRequestModel
    {
        [Fact]
        public void MapsAllProperties_WhenCalled()
        {
            var model = new CreateDummyPlayerRequestModel("test-game-id", "Jess B", 1371);

            var result = model.ToCommand();

            Assert.Equal(model.GameId, result.GameId);
            Assert.Equal(model.DisplayName, result.DisplayName);
            Assert.Equal(model.EstimatedRating, result.EstimatedRating);
        }
    }

    public class ToQuery
    {
        [Fact]
        public void MapsAllSimpleProperties_WhenCalled()
        {
            var model = new GetPlayersRequestModel(
                GameId: "test-game-id",
                DisplayName: "Jess B",
                UserId: "test-user-id",
                RatingFrom: 900,
                RatingTo: 1100,
                Team: nameof(GameTeamEnum.Home),
                Type: nameof(PlayerTypeEnum.User),
                CreatedFrom: new DateTime(2026, 1, 2),
                CreatedTo: new DateTime(2026, 1, 3),
                ModifiedFrom: new DateTime(2026, 1, 4),
                ModifiedTo: new DateTime(2026, 1, 5),
                PageSize: 25,
                Cursor: null);

            var result = model.ToQuery();

            Assert.Equal(model.GameId, result.GameId);
            Assert.Equal(model.DisplayName, result.DisplayName);
            Assert.Equal(model.UserId, result.UserId);
            Assert.Equal(model.RatingFrom, result.RatingFrom);
            Assert.Equal(model.RatingTo, result.RatingTo);
            Assert.Equal(GameTeamEnum.Home, result.Team);
            Assert.Equal(PlayerTypeEnum.User, result.Type);
            Assert.Equal(model.CreatedFrom, result.CreatedFrom);
            Assert.Equal(model.CreatedTo, result.CreatedTo);
            Assert.Equal(model.ModifiedFrom, result.ModifiedFrom);
            Assert.Equal(model.ModifiedTo, result.ModifiedTo);
            Assert.Equal(model.PageSize, result.PageSize);
        }

        [Theory]
        [InlineData("Home", GameTeamEnum.Home)]
        [InlineData("away", GameTeamEnum.Away)]
        public void ParsesTeamCaseInsensitively_WhenValid(string team, GameTeamEnum expected)
        {
            var model = new GetPlayersRequestModel(Team: team);

            var result = model.ToQuery();

            Assert.Equal(expected, result.Team);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("NotARealTeam")]
        public void SetsTeamToNull_WhenInvalidOrMissing(string? team)
        {
            var model = new GetPlayersRequestModel(Team: team);

            var result = model.ToQuery();

            Assert.Null(result.Team);
        }

        [Theory]
        [InlineData("User", PlayerTypeEnum.User)]
        [InlineData("dummy", PlayerTypeEnum.Dummy)]
        public void ParsesTypeCaseInsensitively_WhenValid(string type, PlayerTypeEnum expected)
        {
            var model = new GetPlayersRequestModel(Type: type);

            var result = model.ToQuery();

            Assert.Equal(expected, result.Type);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("NotARealType")]
        public void SetsTypeToNull_WhenInvalidOrMissing(string? type)
        {
            var model = new GetPlayersRequestModel(Type: type);

            var result = model.ToQuery();

            Assert.Null(result.Type);
        }

        [Fact]
        public void SetsCursorToNull_WhenCursorIsNull()
        {
            var model = new GetPlayersRequestModel(Cursor: null);

            var result = model.ToQuery();

            Assert.Null(result.Cursor);
        }

        [Fact]
        public void SetsCursorToNull_WhenCursorIsInvalid()
        {
            var model = new GetPlayersRequestModel(Cursor: "not-a-valid-cursor!!");

            var result = model.ToQuery();

            Assert.Null(result.Cursor);
        }

        [Fact]
        public void DecodesCursor_WhenCursorIsValid()
        {
            ((long?)12345).TryEncodeCursor(out var encodedCursor);
            var model = new GetPlayersRequestModel(Cursor: encodedCursor);

            var result = model.ToQuery();

            Assert.Equal(12345, result.Cursor);
        }
    }
}