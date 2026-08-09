using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;
using Teams.Api.Controllers.V1.Games;
using Teams.Api.Controllers.V1.Games.RequestModels;
using Teams.Api.Controllers.V1.Games.ResponseModels;
using Teams.Common.Pagination;
using Teams.Common.Providers.Identifiers;
using Teams.Core.CQRS;
using Teams.Core.UseCases.Games.CreateGame;
using Teams.Core.UseCases.Games.DeleteGame;
using Teams.Core.UseCases.Games.GenerateTeams;
using Teams.Core.UseCases.Games.GetGameById;
using Teams.Core.UseCases.Games.GetGames;
using Teams.Core.UseCases.Games.RecordResult;
using Teams.Core.UseCases.Games.SetTeams;
using Teams.Core.UseCases.Games.UpdateGame;
using Teams.Domain.Entities;
using Teams.Domain.Enums;
using Teams.Domain.Extensions;
using Teams.Domain.Models;

namespace Teams.Api.UnitTests.Controllers.V1.Games;

/// <summary>
/// Controllers are really tested through integration tests, these really just serve to ensure that we're injecting
/// values into mappers correctly.
/// </summary>
public static class GamesControllerTests
{
    public abstract class GameControllerTestsBase
    {
        protected readonly IMediator Mediator = Substitute.For<IMediator>();

        private GamesController? _sut;

        protected GamesController GetOrCreateSut() =>
            _sut ??= new GamesController(Mediator)
            {
                ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext() }
            };

        protected static Game GetGame(
            string? id = null,
            string? organiserId = null,
            string? location = "Test Venue",
            DateTime? startTime = null,
            int duration = 60,
            int teamSize = 5)
        {
            using var idFix = new IdentifierProviderContext(id ?? Guid.NewGuid().ToString("N"));
            return new Game(
                organiserId ?? Guid.NewGuid().ToString("N"),
                location,
                startTime ?? new DateTime(2026, 1, 1, 12, 0, 0, DateTimeKind.Utc),
                duration,
                teamSize);
        }

        protected static Player GetPlayer(
            string? id = null,
            string gameId = "test-game-id",
            string? userId = null,
            string? displayName = null,
            int rating = 1000,
            PlayerTypeEnum type = PlayerTypeEnum.Dummy,
            GameTeamEnum team = GameTeamEnum.None)
        {
            using var idFix = new IdentifierProviderContext(id ?? Guid.NewGuid().ToString("N"));
            return new Player(gameId, userId, displayName ?? "Test Player", rating, type, team);
        }

        protected static void AssertResultValue<TResult, TValue>(IActionResult result, TValue expected)
            where TResult : ObjectResult
        {
            var objectResult = Assert.IsType<TResult>(result);
            var actual = Assert.IsType<TValue>(objectResult.Value);
            Assert.Equivalent(expected, actual);
        }
    }

    public class GetGames : GameControllerTestsBase
    {
        [Fact]
        public async Task ShouldReturnOk_WhenSuccess()
        {
            var request = new GetGamesRequestModel();
            IReadOnlyCollection<Game> games = [GetGame(), GetGame(), GetGame()];

            Mediator.SendAsync(Arg.Any<GetGamesQuery>(), Arg.Any<CancellationToken>())
                .Returns(games);

            var expected = games.ToPagedList(GamesMapper.ToModel);

            var sut = GetOrCreateSut();
            var rawResult = await sut.GetGames(request, TestContext.Current.CancellationToken);

            AssertResultValue<OkObjectResult, PagedList<GameModel>>(rawResult, expected);

            await Mediator.Received(1).SendAsync(Arg.Any<GetGamesQuery>(), Arg.Any<CancellationToken>());
        }
    }

    public class CreateGame : GameControllerTestsBase
    {
        [Fact]
        public async Task ShouldReturnCreatedAtAction_WhenSuccess()
        {
            const string organiserId = "test-organiser-id";
            var requestModel = new CreateGameRequestModel(
                "Test Venue", new DateTime(2026, 2, 1, 19, 0, 0, DateTimeKind.Utc), 60, 5, organiserId);

            var game = GetGame(
                organiserId: organiserId,
                location: requestModel.Location,
                startTime: requestModel.StartTime,
                duration: requestModel.Duration,
                teamSize: requestModel.TeamSize);

            Mediator.SendAsync(Arg.Any<CreateGameCommand>(), Arg.Any<CancellationToken>())
                .Returns(game);

            var expected = game.ToModel();

            var sut = GetOrCreateSut();
            var rawResult = await sut.CreateGame(requestModel, TestContext.Current.CancellationToken);

            var createdResult = Assert.IsType<CreatedAtActionResult>(rawResult);
            var actual = Assert.IsType<GameModel>(createdResult.Value);
            Assert.Equivalent(expected, actual);
            Assert.Equal(nameof(GamesController.GetGameById), createdResult.ActionName);
            Assert.Equal(game.Id, createdResult.RouteValues?["id"]);

            await Mediator.Received(1).SendAsync(
                Arg.Is<CreateGameCommand>(c =>
                    c.OrganiserId == organiserId &&
                    c.Location == requestModel.Location &&
                    c.StartTime == requestModel.StartTime &&
                    c.Duration == requestModel.Duration &&
                    c.TeamSize == requestModel.TeamSize),
                Arg.Any<CancellationToken>());
        }
    }

    public class GetGameById : GameControllerTestsBase
    {
        [Fact]
        public async Task ShouldReturnOk_WhenSuccess()
        {
            const string id = "test-id";
            var game = GetGame(id: id);

            Mediator.SendAsync(Arg.Any<GetGameByIdQuery>(), Arg.Any<CancellationToken>())
                .Returns(game);

            var expected = game.ToDetailedModel();

            var sut = GetOrCreateSut();
            var rawResult = await sut.GetGameById(id, TestContext.Current.CancellationToken);

            AssertResultValue<OkObjectResult, GameDetailModel>(rawResult, expected);

            await Mediator.Received(1).SendAsync(Arg.Is<GetGameByIdQuery>(q => q.Id == id), Arg.Any<CancellationToken>());
        }
    }

    public class UpdateGame : GameControllerTestsBase
    {
        [Fact]
        public async Task ShouldReturnNoContent_WhenSuccess()
        {
            const string id = "test-id";
            var requestModel = new UpdateGameRequestModel(
                "New Venue", new DateTime(2026, 3, 1, 18, 30, 0, DateTimeKind.Utc), 90);

            Mediator.SendAsync(Arg.Any<UpdateGameCommand>(), Arg.Any<CancellationToken>())
                .Returns(GetGame(id: id));

            var sut = GetOrCreateSut();
            var rawResult = await sut.UpdateGame(id, requestModel, TestContext.Current.CancellationToken);

            Assert.IsType<NoContentResult>(rawResult);

            await Mediator.Received(1).SendAsync(
                Arg.Is<UpdateGameCommand>(c =>
                    c.Id == id &&
                    c.Location == requestModel.Location &&
                    c.StartTime == requestModel.StartTime &&
                    c.Duration == requestModel.Duration),
                Arg.Any<CancellationToken>());
        }
    }

    public class DeleteGame : GameControllerTestsBase
    {
        [Fact]
        public async Task ShouldReturnNoContent_WhenSuccess()
        {
            const string id = "test-id";

            Mediator.SendAsync(Arg.Any<DeleteGameCommand>(), Arg.Any<CancellationToken>())
                .Returns(GetGame(id: id));

            var sut = GetOrCreateSut();
            var rawResult = await sut.DeleteGame(id, TestContext.Current.CancellationToken);

            Assert.IsType<NoContentResult>(rawResult);

            await Mediator.Received(1).SendAsync(Arg.Is<DeleteGameCommand>(c => c.Id == id), Arg.Any<CancellationToken>());
        }
    }

    public class RecordResult : GameControllerTestsBase
    {
        [Fact]
        public async Task ShouldReturnNoContent_WhenSuccess()
        {
            const string id = "test-id";
            var requestModel = new RecordResultRequestModel(nameof(GameTeamEnum.Home));

            Mediator.SendAsync(Arg.Any<RecordGameResultCommand>(), Arg.Any<CancellationToken>())
                .Returns(GetGame(id: id));

            var sut = GetOrCreateSut();
            var rawResult = await sut.RecordResult(id, requestModel, TestContext.Current.CancellationToken);

            Assert.IsType<NoContentResult>(rawResult);

            await Mediator.Received(1).SendAsync(
                Arg.Is<RecordGameResultCommand>(c => c.Id == id && c.Winner == requestModel.Winner),
                Arg.Any<CancellationToken>());
        }
    }

    public class GenerateTeams : GameControllerTestsBase
    {
        [Fact]
        public async Task ShouldReturnOkWithSuggestion_WhenSuggestionsFound()
        {
            const string id = "test-id";
            var requestModel = new GenerateTeamsRequestModel(["home-seed-id"], ["away-seed-id"], 100);

            var suggestion = new TeamSuggestion(
                [GetPlayer(team: GameTeamEnum.Home)],
                [GetPlayer(team: GameTeamEnum.Away)],
                HomeRating: 1000,
                AwayRating: 950,
                TeamDifferential: 50);

            IReadOnlyCollection<TeamSuggestion> suggestions = [suggestion];

            Mediator.SendAsync(Arg.Any<GenerateTeamsCommand>(), Arg.Any<CancellationToken>())
                .Returns(suggestions);

            var expected = suggestion.ToModel(id);

            var sut = GetOrCreateSut();
            var rawResult = await sut.GenerateTeams(id, requestModel, TestContext.Current.CancellationToken);

            AssertResultValue<OkObjectResult, GameTeamsModel>(rawResult, expected);

            await Mediator.Received(1).SendAsync(
                Arg.Is<GenerateTeamsCommand>(c =>
                    c.GameId == id &&
                    c.Differential == requestModel.Differential &&
                    c.Count == 1),
                Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task ShouldReturnOkWithNoBody_WhenNoSuggestionsFound()
        {
            const string id = "test-id";
            var requestModel = new GenerateTeamsRequestModel([], [], 0);
            IReadOnlyCollection<TeamSuggestion> suggestions = [];

            Mediator.SendAsync(Arg.Any<GenerateTeamsCommand>(), Arg.Any<CancellationToken>())
                .Returns(suggestions);

            var sut = GetOrCreateSut();
            var rawResult = await sut.GenerateTeams(id, requestModel, TestContext.Current.CancellationToken);

            var okResult = Assert.IsType<OkResult>(rawResult);
            Assert.Equal(200, okResult.StatusCode);
        }
    }

    public class GetTeams : GameControllerTestsBase
    {
        [Fact]
        public async Task ShouldReturnOk_WhenSuccess()
        {
            const string id = "test-id";
            var game = GetGame(id: id);
            game.Players.Add(GetPlayer(gameId: id, team: GameTeamEnum.Home));
            game.Players.Add(GetPlayer(gameId: id, team: GameTeamEnum.Away));

            Mediator.SendAsync(Arg.Any<GetGameByIdQuery>(), Arg.Any<CancellationToken>())
                .Returns(game);

            var expected = game.ToTeamsModel();

            var sut = GetOrCreateSut();
            var rawResult = await sut.GetTeams(id, TestContext.Current.CancellationToken);

            AssertResultValue<OkObjectResult, GameTeamsModel>(rawResult, expected);

            await Mediator.Received(1).SendAsync(Arg.Is<GetGameByIdQuery>(q => q.Id == id), Arg.Any<CancellationToken>());
        }
    }

    public class SetTeams : GameControllerTestsBase
    {
        [Fact]
        public async Task ShouldReturnNoContent_WhenSuccess()
        {
            const string id = "test-id";
            var requestModel = new SetTeamsRequestModel(["home-id-1", "home-id-2"], ["away-id-1", "away-id-2"]);

            Mediator.SendAsync(Arg.Any<SetTeamsCommand>(), Arg.Any<CancellationToken>())
                .Returns(GetGame(id: id));

            var sut = GetOrCreateSut();
            var rawResult = await sut.SetTeams(id, requestModel, TestContext.Current.CancellationToken);

            Assert.IsType<NoContentResult>(rawResult);

            await Mediator.Received(1).SendAsync(
                Arg.Is<SetTeamsCommand>(c =>
                    c.GameId == id &&
                    c.HomeTeamIds.SequenceEqual(requestModel.HomeTeamIds) &&
                    c.AwayTeamIds.SequenceEqual(requestModel.AwayTeamIds)),
                Arg.Any<CancellationToken>());
        }
    }

    public class ClearTeams : GameControllerTestsBase
    {
        [Fact]
        public async Task ShouldReturnNoContent_WhenSuccess()
        {
            const string id = "test-id";

            Mediator.SendAsync(Arg.Any<SetTeamsCommand>(), Arg.Any<CancellationToken>())
                .Returns(GetGame(id: id));

            var sut = GetOrCreateSut();
            var rawResult = await sut.ClearTeams(id, TestContext.Current.CancellationToken);

            Assert.IsType<NoContentResult>(rawResult);

            await Mediator.Received(1).SendAsync(
                Arg.Is<SetTeamsCommand>(c => c.GameId == id && c.HomeTeamIds.Count == 0 && c.AwayTeamIds.Count == 0),
                Arg.Any<CancellationToken>());
        }
    }
}