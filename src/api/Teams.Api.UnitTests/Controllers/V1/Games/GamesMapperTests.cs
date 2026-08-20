using Teams.Api.Controllers.V1.Games;
using Teams.Api.Controllers.V1.Games.RequestModels;
using Teams.Common.Pagination;
using Teams.Common.Providers.Identifiers;
using Teams.Domain.Entities;
using Teams.Domain.Enums;
using Teams.Domain.Models;

namespace Teams.Api.UnitTests.Controllers.V1.Games;

public static class GamesMapperTests
{
    private static Game GetGame(
        string? id = null,
        string? organiserId = null,
        string? location = "Test Venue",
        DateTime? startTime = null,
        int duration = 60,
        int teamSize = 5,
        User? organiser = null)
    {
        organiserId ??= organiser?.Id ?? Guid.NewGuid().ToString("N");
        using var idFix = new IdentifierProviderContext(id ?? Guid.NewGuid().ToString("N"));
        return new Game(
            organiserId,
            location,
            startTime ?? new DateTime(2026, 1, 1, 12, 0, 0, DateTimeKind.Utc),
            duration,
            teamSize)
        {
            Organiser = organiser ?? GetUser(organiserId)
        };
    }

    private static User GetUser(
        string? id = null,
        string? displayName = null,
        string? tag = null)
    {
        using var idFix = new IdentifierProviderContext(id ?? Guid.NewGuid().ToString("N"));
        var user = new User(displayName ?? "Test Organiser", "external-id", "organiser@test.net", null);
        if (tag is not null)
            user.Update(tag, null, null, null);

        return user;
    }

    private static Player GetPlayer(
        string? id = null,
        string gameId = "test-game-id",
        string? userId = null,
        string? displayName = null,
        int rating = 1000,
        PlayerTypeEnum type = PlayerTypeEnum.Dummy,
        GameTeamEnum team = GameTeamEnum.None,
        User? user = null)
    {
        using var idFix = new IdentifierProviderContext(id ?? Guid.NewGuid().ToString("N"));
        return new Player(gameId, userId, rating, type, team)
        {
            DisplayName = userId == null
                ? displayName ?? "Test Player"
                : null,
            User = user
        };
    }

    public class ToModelFromGame
    {
        [Fact]
        public void MapsAllProperties_WhenCalled()
        {
            var game = GetGame(
                location: "Oak Leaf Leisure Centre",
                startTime: new DateTime(2026, 7, 31, 20, 45, 0, DateTimeKind.Utc),
                duration: 60,
                teamSize: 5);

            var result = game.ToModel();

            Assert.Equal(game.Id, result.Id);
            Assert.Equal(game.Location, result.Location);
            Assert.Equal(game.StartTime, result.StartTime);
            Assert.Equal(game.Duration, result.Duration);
            Assert.Equal(game.TeamSize, result.TeamSize);
            Assert.Equal(nameof(GameStatusEnum.Scheduled), result.Status);
            Assert.Equal(game.Organiser!.Id, result.Organiser!.Id);
            Assert.Equal(game.Organiser.Tag, result.Organiser.Tag);
            Assert.Equal(game.Organiser.DisplayName, result.Organiser.DisplayName);
        }

        [Fact]
        public void SetsOrganiserToNull_WhenGameHasNoOrganiser()
        {
            var game = new Game("missing-organiser-id", "Test Venue", new DateTime(2026, 1, 1, 12, 0, 0, DateTimeKind.Utc), 60, 5);

            var result = game.ToModel();

            Assert.Null(result.Organiser);
        }
    }

    public class ToDetailedModel
    {
        [Fact]
        public void MapsAllProperties_WhenGameHasFinished()
        {
            var game = GetGame();
            game.Players.Add(GetPlayer(gameId: game.Id, rating: 500, team: GameTeamEnum.Home));
            game.Players.Add(GetPlayer(gameId: game.Id, rating: 400, team: GameTeamEnum.Away));
            game.SetResult(GameTeamEnum.Home);

            var result = game.ToDetailedModel();

            Assert.Equal(game.Id, result.Id);
            Assert.Equal(game.Location, result.Location);
            Assert.Equal(game.StartTime, result.StartTime);
            Assert.Equal(game.Duration, result.Duration);
            Assert.Equal(game.TeamSize, result.TeamSize);
            Assert.Equal(nameof(GameStatusEnum.Finished), result.Status);
            Assert.Equal(nameof(GameTeamEnum.Home), result.Winner);
            Assert.Equal(game.HomeTeamRating, result.HomeTeamRating);
            Assert.Equal(game.AwayTeamRating, result.AwayTeamRating);
            Assert.Equal(game.DateCreated, result.Created);
            Assert.Equal(game.DateModified, result.Modified);
            Assert.Equal(game.Organiser!.Id, result.Organiser!.Id);
            Assert.Equal(game.Organiser.Tag, result.Organiser.Tag);
            Assert.Equal(game.Organiser.DisplayName, result.Organiser.DisplayName);
        }

        [Fact]
        public void SetsOrganiserToNull_WhenGameHasNoOrganiser()
        {
            var game = new Game("missing-organiser-id", "Test Venue", new DateTime(2026, 1, 1, 12, 0, 0, DateTimeKind.Utc), 60, 5);

            var result = game.ToDetailedModel();

            Assert.Null(result.Organiser);
        }

        [Fact]
        public void SetsWinnerAndRatingsToNull_WhenGameHasNotFinished()
        {
            var game = GetGame();

            var result = game.ToDetailedModel();

            Assert.Equal(nameof(GameStatusEnum.Scheduled), result.Status);
            Assert.Null(result.Winner);
            Assert.Null(result.HomeTeamRating);
            Assert.Null(result.AwayTeamRating);
        }
    }

    public class ToTeamsModel
    {
        [Fact]
        public void MapsHomeAndAwayPlayersAndExcludesUnassigned_WhenCalled()
        {
            var game = GetGame();
            var homePlayer = GetPlayer(gameId: game.Id, displayName: "Home Player", rating: 900, team: GameTeamEnum.Home);
            var awayPlayer = GetPlayer(gameId: game.Id, displayName: "Away Player", rating: 850, team: GameTeamEnum.Away);
            var unassignedPlayer = GetPlayer(gameId: game.Id, team: GameTeamEnum.None);
            game.Players.Add(homePlayer);
            game.Players.Add(awayPlayer);
            game.Players.Add(unassignedPlayer);
            game.UpdateHomeTeamRating();
            game.UpdateAwayTeamRating();

            var result = game.ToTeamsModel();

            Assert.Equal(game.Id, result.Id);

            var resultHomePlayer = Assert.Single(result.Home!.Players);
            Assert.Equal(homePlayer.Id, resultHomePlayer.Id);
            Assert.Equal(homePlayer.GetDisplayName(), resultHomePlayer.DisplayName);
            Assert.Equal(homePlayer.Rating, resultHomePlayer.Rating);
            Assert.Equal(game.HomeTeamRating, result.Home.TeamRating);

            var resultAwayPlayer = Assert.Single(result.Away!.Players);
            Assert.Equal(awayPlayer.Id, resultAwayPlayer.Id);
            Assert.Equal(game.AwayTeamRating, result.Away.TeamRating);
        }

        [Fact]
        public void SetsTeamRatingToZero_WhenGameHasNoRatingYet()
        {
            var game = GetGame();

            var result = game.ToTeamsModel();

            Assert.Equal(0, result.Home!.TeamRating);
            Assert.Equal(0, result.Away!.TeamRating);
            Assert.Empty(result.Home.Players);
            Assert.Empty(result.Away.Players);
        }
    }

    public class ToModelFromTeamSuggestion
    {
        [Fact]
        public void MapsGameIdHomeAndAwayTeams_WhenCalled()
        {
            var homePlayer = GetPlayer(displayName: "Home Player", rating: 1000, team: GameTeamEnum.Home);
            var awayPlayer = GetPlayer(displayName: "Away Player", rating: 950, team: GameTeamEnum.Away);
            var suggestion = new TeamSuggestion([homePlayer], [awayPlayer], HomeRating: 1000, AwayRating: 950, TeamDifferential: 50);

            var result = suggestion.ToModel("test-game-id");

            Assert.Equal("test-game-id", result.Id);

            var resultHomePlayer = Assert.Single(result.Home!.Players);
            Assert.Equal(homePlayer.Id, resultHomePlayer.Id);
            Assert.Equal(suggestion.HomeRating, result.Home.TeamRating);

            var resultAwayPlayer = Assert.Single(result.Away!.Players);
            Assert.Equal(awayPlayer.Id, resultAwayPlayer.Id);
            Assert.Equal(suggestion.AwayRating, result.Away.TeamRating);
        }
    }

    public class ToGameTeamPlayerModel
    {
        [Fact]
        public void MapsIdDisplayNameAndRating_WhenCalled()
        {
            var player = GetPlayer(displayName: "Test Player", rating: 777);

            var result = player.ToGameTeamPlayerModel();

            Assert.Equal(player.Id, result.Id);
            Assert.Equal(player.GetDisplayName(), result.DisplayName);
            Assert.Equal(player.Rating, result.Rating);
        }

        [Fact]
        public void MapsTagFromLinkedUser_WhenPlayerIsUserLinked()
        {
            var user = GetUser(tag: "marcusaurelius");
            var player = GetPlayer(userId: user.Id, user: user, type: PlayerTypeEnum.User);

            var result = player.ToGameTeamPlayerModel();

            Assert.Equal(user.Tag, result.Tag);
        }

        [Fact]
        public void SetsTagToNull_WhenPlayerHasNoLinkedUser()
        {
            var player = GetPlayer();

            var result = player.ToGameTeamPlayerModel();

            Assert.Null(result.Tag);
        }
    }

    public class ToCommandFromCreateGameRequestModel
    {
        [Fact]
        public void MapsAllProperties_WhenCalled()
        {
            var model = new CreateGameRequestModel(
                Location: "Oak Leaf Leisure Centre",
                StartTime: new DateTime(2026, 7, 31, 20, 45, 0, DateTimeKind.Utc),
                Duration: 60,
                TeamSize: 5,
                OrganiserId: "test-organiser-id");

            var result = model.ToCommand();

            Assert.Equal(model.OrganiserId, result.OrganiserId);
            Assert.Equal(model.Location, result.Location);
            Assert.Equal(model.StartTime, result.StartTime);
            Assert.Equal(model.Duration, result.Duration);
            Assert.Equal(model.TeamSize, result.TeamSize);
        }
    }

    public class ToCommandFromUpdateGameRequestModel
    {
        [Fact]
        public void MapsIdAndProvidedProperties_WhenCalled()
        {
            var model = new UpdateGameRequestModel("New Venue", new DateTime(2026, 3, 1, 18, 30, 0, DateTimeKind.Utc), 90);

            var result = model.ToCommand("test-game-id");

            Assert.Equal("test-game-id", result.Id);
            Assert.Equal(model.Location, result.Location);
            Assert.Equal(model.StartTime, result.StartTime);
            Assert.Equal(model.Duration, result.Duration);
        }

        [Fact]
        public void SetsPropertiesToNull_WhenModelPropertiesAreNull()
        {
            var model = new UpdateGameRequestModel(null, null, null);

            var result = model.ToCommand("test-game-id");

            Assert.Null(result.Location);
            Assert.Null(result.StartTime);
            Assert.Null(result.Duration);
        }
    }

    public class ToCommandFromGenerateTeamsRequestModel
    {
        [Fact]
        public void MapsGameIdSeedIdsAndDifferential_WhenCalled()
        {
            var model = new GenerateTeamsRequestModel(["home-seed-1", "home-seed-2"], ["away-seed-1"], 200);

            var result = model.ToCommand("test-game-id");

            Assert.Equal("test-game-id", result.GameId);
            Assert.Equal(model.HomeTeamSeedIds, result.HomeSeedPlayerIds);
            Assert.Equal(model.AwayTeamSeedIds, result.AwaySeedPlayerIds);
            Assert.Equal(model.Differential, result.Differential);
        }

        [Fact]
        public void AlwaysSetsCountToOne_WhenCalled()
        {
            var model = new GenerateTeamsRequestModel([], [], 0);

            var result = model.ToCommand("test-game-id");

            Assert.Equal(1, result.Count);
        }
    }

    public class ToCommandFromSetTeamsRequestModel
    {
        [Fact]
        public void MapsAllProperties_WhenCalled()
        {
            var model = new SetTeamsRequestModel(["home-1", "home-2"], ["away-1", "away-2"]);

            var result = model.ToCommand("test-game-id");

            Assert.Equal("test-game-id", result.GameId);
            Assert.Equal(model.HomeTeamIds, result.HomeTeamIds);
            Assert.Equal(model.AwayTeamIds, result.AwayTeamIds);
        }
    }

    public class ToCommandFromRecordResultRequestModel
    {
        [Fact]
        public void MapsIdAndWinner_WhenCalled()
        {
            var model = new RecordResultRequestModel(nameof(GameTeamEnum.Away));

            var result = model.ToCommand("test-game-id");

            Assert.Equal("test-game-id", result.Id);
            Assert.Equal(model.Winner, result.Winner);
        }
    }

    public class ToQuery
    {
        [Fact]
        public void MapsAllSimpleProperties_WhenCalled()
        {
            var model = new GetGamesRequestModel(
                Location: "Oak Leaf Leisure Centre",
                StartTimeFrom: new DateTime(2026, 1, 1),
                StartTimeTo: new DateTime(2026, 1, 31),
                DurationFrom: 30,
                DurationTo: 90,
                TeamSize: 5,
                Status: nameof(GameStatusEnum.Finished),
                CreatedFrom: new DateTime(2026, 1, 2),
                CreatedTo: new DateTime(2026, 1, 3),
                ModifiedFrom: new DateTime(2026, 1, 4),
                ModifiedTo: new DateTime(2026, 1, 5),
                PageSize: 25,
                Cursor: null);

            var result = model.ToQuery();

            Assert.Equal(model.Location, result.Location);
            Assert.Equal(model.StartTimeFrom, result.StartTimeFrom);
            Assert.Equal(model.StartTimeTo, result.StartTimeTo);
            Assert.Equal(model.DurationFrom, result.DurationFrom);
            Assert.Equal(model.DurationTo, result.DurationTo);
            Assert.Equal(model.TeamSize, result.TeamSize);
            Assert.Equal(model.CreatedFrom, result.CreatedFrom);
            Assert.Equal(model.CreatedTo, result.CreatedTo);
            Assert.Equal(model.ModifiedFrom, result.ModifiedFrom);
            Assert.Equal(model.ModifiedTo, result.ModifiedTo);
            Assert.Equal(model.PageSize, result.PageSize);
        }

        [Theory]
        [InlineData("Scheduled", GameStatusEnum.Scheduled)]
        [InlineData("finished", GameStatusEnum.Finished)]
        public void ParsesStatusCaseInsensitively_WhenValid(string status, GameStatusEnum expected)
        {
            var model = new GetGamesRequestModel(Status: status);

            var result = model.ToQuery();

            Assert.Equal(expected, result.Status);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("NotARealStatus")]
        public void SetsStatusToNull_WhenInvalidOrMissing(string? status)
        {
            var model = new GetGamesRequestModel(Status: status);

            var result = model.ToQuery();

            Assert.Null(result.Status);
        }

        [Fact]
        public void SetsCursorToNull_WhenCursorIsNull()
        {
            var model = new GetGamesRequestModel(Cursor: null);

            var result = model.ToQuery();

            Assert.Null(result.Cursor);
        }

        [Fact]
        public void SetsCursorToNull_WhenCursorIsInvalid()
        {
            var model = new GetGamesRequestModel(Cursor: "not-a-valid-cursor!!");

            var result = model.ToQuery();

            Assert.Null(result.Cursor);
        }

        [Fact]
        public void DecodesCursor_WhenCursorIsValid()
        {
            ((long?)12345).TryEncodeCursor(out var encodedCursor);
            var model = new GetGamesRequestModel(Cursor: encodedCursor);

            var result = model.ToQuery();

            Assert.Equal(12345, result.Cursor);
        }
    }
}