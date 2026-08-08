using Microsoft.Extensions.DependencyInjection;
using System.Net;
using Teams.Api.Controllers.V1.Games.RequestModels;
using Teams.Api.Controllers.V1.Games.ResponseModels;
using Teams.Api.Controllers.V1.Players.ResponseModel;
using Teams.Api.Controllers.V1.Users.ResponseModels;
using Teams.Data.Context;
using Teams.Domain.Entities;
using Teams.Domain.Enums;

namespace Teams.Api.IntegrationTests.Controllers.V1.Games;

public static partial class GamesControllerTests
{
    public class RecordResult(ApiWebApplicationFactory factory) : GamesControllerTestsBase(factory)
    {
        private static RecordResultRequestModel ValidRequest => new(nameof(GameTeamEnum.Home));

        [Fact]
        public async Task ShouldReturnBadRequest_WhenVersionIsUnsupported()
        {
            var existingGame = SeedGames[0];
            var organiser = SeedOrganisers.Single(u => u.Id == existingGame.OrganiserId);

            var request = CreateJsonRequest(
                HttpMethod.Post,
                $"{VersionlessUrl}/{existingGame.Id}/result",
                ValidRequest,
                apiVersion: "2.0");
            WithActorHeaders(request, organiser);

            var response = await Client.SendAsync(request, TestContext.Current.CancellationToken);

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task ShouldReturnBadRequest_WhenVersionIsNotProvided()
        {
            var existingGame = SeedGames[0];
            var organiser = SeedOrganisers.Single(u => u.Id == existingGame.OrganiserId);

            var request = CreateJsonRequest(
                HttpMethod.Post,
                $"{VersionlessUrl}/{existingGame.Id}/result",
                ValidRequest,
                apiVersion: null);
            WithActorHeaders(request, organiser);

            var response = await Client.SendAsync(request, TestContext.Current.CancellationToken);

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task ShouldReturnPreconditionRequired_WhenActorHeadersAreMissing()
        {
            var existingGame = SeedGames[0];

            var request = CreateJsonRequest(HttpMethod.Post, $"{Url}/{existingGame.Id}/result", ValidRequest);

            var response = await Client.SendAsync(request, TestContext.Current.CancellationToken);
            var problem = await ReadProblemDetailsAsync(response, TestContext.Current.CancellationToken);

            Assert.Equal(HttpStatusCode.PreconditionRequired, response.StatusCode);
            Assert.NotNull(problem);
            Assert.Equal("'Teams-User-Id' header value is required.", problem.Detail);
        }

        [Fact]
        public async Task ShouldReturnForbidden_WhenActorIsNotOrganiser()
        {
            var existingGame = SeedGames[0];
            var nonOrganiser = SeedOrganisers.First(u => u.Id != existingGame.OrganiserId);

            var request = CreateJsonRequest(HttpMethod.Post, $"{Url}/{existingGame.Id}/result", ValidRequest);
            WithActorHeaders(request, nonOrganiser);

            var response = await Client.SendAsync(request, TestContext.Current.CancellationToken);
            var problem = await ReadProblemDetailsAsync(response, TestContext.Current.CancellationToken);

            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
            Assert.NotNull(problem);
            Assert.Equal("Action only available to game organiser.", problem.Detail);
        }

        [Fact]
        public async Task ShouldReturnNotFound_WhenGameDoesNotExist()
        {
            const string id = "does-not-exist";

            var request = CreateJsonRequest(HttpMethod.Post, $"{Url}/{id}/result", ValidRequest);
            WithActorHeaders(request, SeedOrganisers[0]);

            var response = await Client.SendAsync(request, TestContext.Current.CancellationToken);
            var problem = await ReadProblemDetailsAsync(response, TestContext.Current.CancellationToken);

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
            Assert.NotNull(problem);
            Assert.Equal(nameof(Game), problem.Extensions["resource"]?.ToString());
            Assert.Equal(id, problem.Extensions["identifier"]?.ToString());
        }

        [Fact]
        public async Task ShouldReturnUnprocessableEntity_WhenWinnerIsInvalid()
        {
            var existingGame = SeedGames[0];
            var organiser = SeedOrganisers.Single(u => u.Id == existingGame.OrganiserId);
            var invalidRequest = new RecordResultRequestModel(Winner: "NotARealTeam");

            var request = CreateJsonRequest(HttpMethod.Post, $"{Url}/{existingGame.Id}/result", invalidRequest);
            WithActorHeaders(request, organiser);

            var response = await Client.SendAsync(request, TestContext.Current.CancellationToken);
            var problem = await ReadProblemDetailsAsync(response, TestContext.Current.CancellationToken);

            Assert.Equal(HttpStatusCode.UnprocessableEntity, response.StatusCode);
            Assert.NotNull(problem);
            Assert.NotEmpty(GetValidationErrors(problem, nameof(RecordResultRequestModel.Winner)));
        }

        [Fact]
        public async Task ShouldNotChangeWinner_WhenGameIsAlreadyFinished()
        {
            var finishedGame = SeedGames[1]; // seed game 2 - even index, already finished (winner: Away)
            var organiser = SeedOrganisers.Single(u => u.Id == finishedGame.OrganiserId);
            var conflictingRequest = new RecordResultRequestModel(Winner: nameof(GameTeamEnum.Home));

            var request = CreateJsonRequest(HttpMethod.Post, $"{Url}/{finishedGame.Id}/result", conflictingRequest);
            WithActorHeaders(request, organiser);

            var response = await Client.SendAsync(request, TestContext.Current.CancellationToken);

            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

            var getRequest = CreateRequest(HttpMethod.Get, $"{Url}/{finishedGame.Id}");
            var getResponse = await Client.SendAsync(getRequest, TestContext.Current.CancellationToken);
            var detail = await ReadContentAsync<GameDetailModel>(getResponse, TestContext.Current.CancellationToken);

            Assert.NotNull(detail);
            Assert.Equal(finishedGame.Winner?.ToString(), detail.Winner); // still "Away" - unchanged by the "Home" request
        }

        private async Task<(User LinkedUser, Game Game, Player HomePlayer, Player AwayPlayer)> SeedGameWithPlayersAsync(User organiser)
        {
            var linkedUser = EntityFactory.CreateUser(displayName: "Linked Player");
            var game = EntityFactory.CreateGame(organiser.Id, teamSize: 3);
            var homePlayer = EntityFactory.CreatePlayer(
                game.Id, userId: linkedUser.Id, displayName: linkedUser.DisplayName, rating: linkedUser.Rating,
                type: PlayerTypeEnum.User, team: GameTeamEnum.Home);
            var homeTeammate = EntityFactory.CreatePlayer(game.Id, displayName: "Dummy Home Player", rating: 1000, team: GameTeamEnum.Home);
            var awayPlayer = EntityFactory.CreatePlayer(
                game.Id, displayName: "Dummy Away Player", rating: 900, team: GameTeamEnum.Away);
            var awayTeammate = EntityFactory.CreatePlayer(game.Id, displayName: "Dummy Away Player 2", rating: 900, team: GameTeamEnum.Away);

            await using var scope = Factory.Services.CreateAsyncScope();
            var context = scope.ServiceProvider.GetRequiredService<ApiDbContext>();
            await context.Users.AddAsync(linkedUser, TestContext.Current.CancellationToken);
            await context.Games.AddAsync(game, TestContext.Current.CancellationToken);
            await context.Players.AddRangeAsync(
                [homePlayer, homeTeammate, awayPlayer, awayTeammate], TestContext.Current.CancellationToken);
            await context.SaveChangesAsync(TestContext.Current.CancellationToken);

            return (linkedUser, game, homePlayer, awayPlayer);
        }

        [Fact]
        public async Task ShouldNotReapplyRatingChanges_WhenResultIsRecordedTwice()
        {
            var organiser = SeedOrganisers[0];
            var (linkedUser, game, homePlayer, _) = await SeedGameWithPlayersAsync(organiser);

            var firstRequest = CreateJsonRequest(HttpMethod.Post, $"{Url}/{game.Id}/result", ValidRequest); // Home wins
            WithActorHeaders(firstRequest, organiser);
            await Client.SendAsync(firstRequest, TestContext.Current.CancellationToken);

            var getUserAfterFirstCallRequest = CreateRequest(HttpMethod.Get, $"api/v1/users/{linkedUser.Id}");
            var getUserAfterFirstCallResponse = await Client.SendAsync(getUserAfterFirstCallRequest, TestContext.Current.CancellationToken);
            var userAfterFirstCall = await ReadContentAsync<UserDetailModel>(getUserAfterFirstCallResponse, TestContext.Current.CancellationToken);
            Assert.NotNull(userAfterFirstCall);

            var secondRequest = CreateJsonRequest(HttpMethod.Post, $"{Url}/{game.Id}/result", ValidRequest); // same result, again
            WithActorHeaders(secondRequest, organiser);
            var secondResponse = await Client.SendAsync(secondRequest, TestContext.Current.CancellationToken);

            Assert.Equal(HttpStatusCode.NoContent, secondResponse.StatusCode);

            var getPlayerRequest = CreateRequest(HttpMethod.Get, $"api/v1/players/{homePlayer.Id}");
            var getPlayerResponse = await Client.SendAsync(getPlayerRequest, TestContext.Current.CancellationToken);
            var playerDetail = await ReadContentAsync<PlayerDetailModel>(getPlayerResponse, TestContext.Current.CancellationToken);

            var getUserAfterSecondCallRequest = CreateRequest(HttpMethod.Get, $"api/v1/users/{linkedUser.Id}");
            var getUserAfterSecondCallResponse = await Client.SendAsync(getUserAfterSecondCallRequest, TestContext.Current.CancellationToken);
            var userAfterSecondCall = await ReadContentAsync<UserDetailModel>(getUserAfterSecondCallResponse, TestContext.Current.CancellationToken);

            Assert.NotNull(playerDetail);
            Assert.NotNull(userAfterSecondCall);
            Assert.Equal(userAfterFirstCall.Rating, userAfterSecondCall.Rating); // not doubled by the second call
            Assert.Equal(userAfterSecondCall.Rating, linkedUser.Rating + playerDetail.RatingChange);
        }

        [Fact]
        public async Task ShouldReturnNoContent_WithRatingChangesApplied_WhenRequestIsValid()
        {
            var organiser = SeedOrganisers[0];
            var (linkedUser, game, homePlayer, _) = await SeedGameWithPlayersAsync(organiser);

            var request = CreateJsonRequest(HttpMethod.Post, $"{Url}/{game.Id}/result", ValidRequest); // Home wins
            WithActorHeaders(request, organiser);

            var response = await Client.SendAsync(request, TestContext.Current.CancellationToken);

            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

            var getGameRequest = CreateRequest(HttpMethod.Get, $"{Url}/{game.Id}");
            var getGameResponse = await Client.SendAsync(getGameRequest, TestContext.Current.CancellationToken);
            var gameDetail = await ReadContentAsync<GameDetailModel>(getGameResponse, TestContext.Current.CancellationToken);

            Assert.NotNull(gameDetail);
            Assert.Equal(nameof(GameStatusEnum.Finished), gameDetail.Status);
            Assert.Equal(nameof(GameTeamEnum.Home), gameDetail.Winner);

            var getPlayerRequest = CreateRequest(HttpMethod.Get, $"api/v1/players/{homePlayer.Id}");
            var getPlayerResponse = await Client.SendAsync(getPlayerRequest, TestContext.Current.CancellationToken);
            var playerDetail = await ReadContentAsync<PlayerDetailModel>(getPlayerResponse, TestContext.Current.CancellationToken);

            Assert.NotNull(playerDetail);
            Assert.NotNull(playerDetail.RatingChange);
            Assert.True(playerDetail.RatingChange > 0); // winning team gains rating

            var getUserRequest = CreateRequest(HttpMethod.Get, $"api/v1/users/{linkedUser.Id}");
            var getUserResponse = await Client.SendAsync(getUserRequest, TestContext.Current.CancellationToken);
            var userDetail = await ReadContentAsync<UserDetailModel>(getUserResponse, TestContext.Current.CancellationToken);

            Assert.NotNull(userDetail);
            Assert.Equal(linkedUser.Rating + playerDetail.RatingChange, userDetail.Rating);
        }
    }
}