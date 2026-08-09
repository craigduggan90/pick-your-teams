using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;
using Teams.Api.Controllers.V1.Players;
using Teams.Api.Controllers.V1.Players.RequestModels;
using Teams.Api.Controllers.V1.Players.ResponseModel;
using Teams.Common.Pagination;
using Teams.Common.Providers.Identifiers;
using Teams.Core.CQRS;
using Teams.Core.UseCases.Players.CreateDummyPlayer;
using Teams.Core.UseCases.Players.CreatePlayer;
using Teams.Core.UseCases.Players.DeletePlayer;
using Teams.Core.UseCases.Players.GetPlayerById;
using Teams.Core.UseCases.Players.GetPlayers;
using Teams.Domain.Entities;
using Teams.Domain.Enums;
using Teams.Domain.Extensions;

namespace Teams.Api.UnitTests.Controllers.V1.Players;

/// <summary>
/// Controllers are really tested through integration tests, these really just serve to ensure that we're injecting
/// values into mappers correctly.
/// </summary>
public static class PlayersControllerTests
{
    public abstract class PlayerControllerTestsBase
    {
        protected readonly IMediator Mediator = Substitute.For<IMediator>();

        private PlayersController? _sut;

        protected PlayersController GetOrCreateSut() =>
            _sut ??= new PlayersController(Mediator)
            {
                ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext() }
            };

        protected static Player GetPlayer(
            string? id = null,
            string gameId = "test-game-id",
            string? userId = null,
            string displayName = "Test Player",
            int rating = 1000,
            PlayerTypeEnum type = PlayerTypeEnum.Dummy,
            GameTeamEnum team = GameTeamEnum.None)
        {
            using var idFix = new IdentifierProviderContext(id ?? Guid.NewGuid().ToString("N"));
            return new Player(gameId, userId, rating, type, team)
            {
                DisplayName = userId == null
                    ? displayName
                    : null
            };
        }

        protected static void AssertResultValue<TResult, TValue>(IActionResult result, TValue expected)
            where TResult : ObjectResult
        {
            var objectResult = Assert.IsType<TResult>(result);
            var actual = Assert.IsType<TValue>(objectResult.Value);
            Assert.Equivalent(expected, actual);
        }
    }

    public class GetPlayers : PlayerControllerTestsBase
    {
        [Fact]
        public async Task ShouldReturnOk_WhenSuccess()
        {
            var request = new GetPlayersRequestModel();
            IReadOnlyCollection<Player> players = [GetPlayer(), GetPlayer(), GetPlayer()];

            Mediator.SendAsync(Arg.Any<GetPlayersQuery>(), Arg.Any<CancellationToken>())
                .Returns(players);

            var expected = players.ToPagedList(PlayersMapper.ToPlayerModel);

            var sut = GetOrCreateSut();
            var rawResult = await sut.GetPlayers(request, TestContext.Current.CancellationToken);

            AssertResultValue<OkObjectResult, PagedList<PlayerModel>>(rawResult, expected);

            await Mediator.Received(1).SendAsync(Arg.Any<GetPlayersQuery>(), Arg.Any<CancellationToken>());
        }
    }

    public class CreatePlayer : PlayerControllerTestsBase
    {
        [Fact]
        public async Task ShouldReturnCreatedAtAction_WhenSuccess()
        {
            const string gameId = "test-game-id";
            const string userId = "test-user-id";
            var requestModel = new CreatePlayerRequestModel(gameId, userId);

            var player = GetPlayer(gameId: gameId, userId: userId, type: PlayerTypeEnum.User);

            Mediator.SendAsync(Arg.Any<CreatePlayerCommand>(), Arg.Any<CancellationToken>())
                .Returns(player);

            var expected = player.ToPlayerModel();

            var sut = GetOrCreateSut();
            var rawResult = await sut.CreatePlayer(requestModel, TestContext.Current.CancellationToken);

            var createdResult = Assert.IsType<CreatedAtActionResult>(rawResult);
            var actual = Assert.IsType<PlayerModel>(createdResult.Value);
            Assert.Equivalent(expected, actual);
            Assert.Equal(nameof(PlayersController.GetPlayerById), createdResult.ActionName);
            Assert.Equal(player.Id, createdResult.RouteValues?["id"]);

            await Mediator.Received(1).SendAsync(
                Arg.Is<CreatePlayerCommand>(c => c.GameId == gameId && c.UserId == userId),
                Arg.Any<CancellationToken>());
        }
    }

    public class CreateDummyPlayer : PlayerControllerTestsBase
    {
        [Fact]
        public async Task ShouldReturnCreatedAtAction_WhenSuccess()
        {
            const string gameId = "test-game-id";
            var requestModel = new CreateDummyPlayerRequestModel(gameId, "Jess B", 1371);

            var player = GetPlayer(
                gameId: gameId,
                displayName: requestModel.DisplayName,
                rating: requestModel.EstimatedRating,
                type: PlayerTypeEnum.Dummy);

            Mediator.SendAsync(Arg.Any<CreateDummyPlayerCommand>(), Arg.Any<CancellationToken>())
                .Returns(player);

            var expected = player.ToPlayerModel();

            var sut = GetOrCreateSut();
            var rawResult = await sut.CreateDummyPlayer(requestModel, TestContext.Current.CancellationToken);

            var createdResult = Assert.IsType<CreatedAtActionResult>(rawResult);
            var actual = Assert.IsType<PlayerModel>(createdResult.Value);
            Assert.Equivalent(expected, actual);
            Assert.Equal(nameof(PlayersController.GetPlayerById), createdResult.ActionName);
            Assert.Equal(player.Id, createdResult.RouteValues?["id"]);

            await Mediator.Received(1).SendAsync(
                Arg.Is<CreateDummyPlayerCommand>(c =>
                    c.GameId == gameId &&
                    c.DisplayName == requestModel.DisplayName &&
                    c.EstimatedRating == requestModel.EstimatedRating),
                Arg.Any<CancellationToken>());
        }
    }

    public class GetPlayerById : PlayerControllerTestsBase
    {
        [Fact]
        public async Task ShouldReturnOk_WhenSuccess()
        {
            const string id = "test-id";
            var player = GetPlayer(id: id);

            Mediator.SendAsync(Arg.Any<GetPlayerByIdQuery>(), Arg.Any<CancellationToken>())
                .Returns(player);

            var expected = player.ToPlayerDetailModel();

            var sut = GetOrCreateSut();
            var rawResult = await sut.GetPlayerById(id, TestContext.Current.CancellationToken);

            AssertResultValue<OkObjectResult, PlayerDetailModel>(rawResult, expected);

            await Mediator.Received(1).SendAsync(Arg.Is<GetPlayerByIdQuery>(q => q.Id == id), Arg.Any<CancellationToken>());
        }
    }

    public class DeletePlayer : PlayerControllerTestsBase
    {
        [Fact]
        public async Task ShouldReturnNoContent_WhenSuccess()
        {
            const string id = "test-id";

            Mediator.SendAsync(Arg.Any<DeletePlayerCommand>(), Arg.Any<CancellationToken>())
                .Returns(GetPlayer(id: id));

            var sut = GetOrCreateSut();
            var rawResult = await sut.DeletePlayer(id, TestContext.Current.CancellationToken);

            Assert.IsType<NoContentResult>(rawResult);

            await Mediator.Received(1).SendAsync(Arg.Is<DeletePlayerCommand>(c => c.Id == id), Arg.Any<CancellationToken>());
        }
    }
}