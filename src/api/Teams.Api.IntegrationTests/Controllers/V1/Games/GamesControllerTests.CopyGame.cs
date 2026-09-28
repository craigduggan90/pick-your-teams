using Microsoft.Extensions.DependencyInjection;
using System.Net;
using Teams.Api.Controllers.V1.Games.RequestModels;
using Teams.Api.Controllers.V1.Games.ResponseModels;
using Teams.Core.UseCases.Games.CopyGame;
using Teams.Data.Context;
using Teams.Domain.Entities;
using Teams.Domain.Enums;

namespace Teams.Api.IntegrationTests.Controllers.V1.Games;

public static partial class GamesControllerTests
{
    public class CopyGame(ApiWebApplicationFactory factory) : GamesControllerTestsBase(factory)
    {
        private static CopyGameRequestModel ValidRequest => new(new DateTime(2026, 9, 7, 20, 45, 0, DateTimeKind.Utc));

        [Fact]
        public async Task ShouldReturnBadRequest_WhenVersionIsUnsupported()
        {
            var finishedGame = SeedGames[1]; // seed game 2 - even index, already finished (winner: Away)
            var organiser = SeedOrganisers.Single(u => u.Id == finishedGame.OrganiserId);

            var request = CreateJsonRequest(
                HttpMethod.Post, $"{VersionlessUrl}/{finishedGame.Id}/copy", ValidRequest, apiVersion: "2.0");
            WithActorHeaders(request, organiser);

            var response = await Client.SendAsync(request, TestContext.Current.CancellationToken);

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task ShouldReturnBadRequest_WhenVersionIsNotProvided()
        {
            var finishedGame = SeedGames[1];
            var organiser = SeedOrganisers.Single(u => u.Id == finishedGame.OrganiserId);

            var request = CreateJsonRequest(
                HttpMethod.Post, $"{VersionlessUrl}/{finishedGame.Id}/copy", ValidRequest, apiVersion: null);
            WithActorHeaders(request, organiser);

            var response = await Client.SendAsync(request, TestContext.Current.CancellationToken);

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task ShouldReturnPreconditionRequired_WhenActorHeadersAreMissing()
        {
            var finishedGame = SeedGames[1];

            var request = CreateJsonRequest(HttpMethod.Post, $"{Url}/{finishedGame.Id}/copy", ValidRequest);

            var response = await Client.SendAsync(request, TestContext.Current.CancellationToken);
            var problem = await ReadProblemDetailsAsync(response, TestContext.Current.CancellationToken);

            Assert.Equal(HttpStatusCode.PreconditionRequired, response.StatusCode);
            Assert.NotNull(problem);
            Assert.Equal("'Teams-User-Id' header value is required.", problem.Detail);
        }

        [Fact]
        public async Task ShouldReturnForbidden_WhenActorIsNotOrganiser()
        {
            var finishedGame = SeedGames[1];
            var nonOrganiser = SeedOrganisers.First(u => u.Id != finishedGame.OrganiserId);

            var request = CreateJsonRequest(HttpMethod.Post, $"{Url}/{finishedGame.Id}/copy", ValidRequest);
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

            var request = CreateJsonRequest(HttpMethod.Post, $"{Url}/{id}/copy", ValidRequest);
            WithActorHeaders(request, SeedOrganisers[0]);

            var response = await Client.SendAsync(request, TestContext.Current.CancellationToken);
            var problem = await ReadProblemDetailsAsync(response, TestContext.Current.CancellationToken);

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
            Assert.NotNull(problem);
            Assert.Equal(nameof(Game), problem.Extensions["resource"]?.ToString());
            Assert.Equal(id, problem.Extensions["identifier"]?.ToString());
        }

        [Fact]
        public async Task ShouldReturnUnprocessableEntity_WhenSourceGameIsNotFinished()
        {
            var scheduledGame = SeedGames[0]; // seed game 1 - odd index, still Scheduled
            var organiser = SeedOrganisers.Single(u => u.Id == scheduledGame.OrganiserId);

            var request = CreateJsonRequest(HttpMethod.Post, $"{Url}/{scheduledGame.Id}/copy", ValidRequest);
            WithActorHeaders(request, organiser);

            var response = await Client.SendAsync(request, TestContext.Current.CancellationToken);
            var problem = await ReadProblemDetailsAsync(response, TestContext.Current.CancellationToken);

            Assert.Equal(HttpStatusCode.UnprocessableEntity, response.StatusCode);
            Assert.NotNull(problem);
            Assert.NotEmpty(GetValidationErrors(problem, nameof(CopyGameCommand.Id)));
        }

        [Fact]
        public async Task ShouldReturnCreated_WithSourceGameSettingsCopiedAndNewStartTime_WhenRequestIsValid()
        {
            var finishedGame = SeedGames[1];
            var organiser = SeedOrganisers.Single(u => u.Id == finishedGame.OrganiserId);

            var request = CreateJsonRequest(HttpMethod.Post, $"{Url}/{finishedGame.Id}/copy", ValidRequest);
            WithActorHeaders(request, organiser);

            var response = await Client.SendAsync(request, TestContext.Current.CancellationToken);
            var content = await ReadContentAsync<GameModel>(response, TestContext.Current.CancellationToken);

            Assert.Equal(HttpStatusCode.Created, response.StatusCode);
            Assert.NotNull(content);
            Assert.NotEqual(finishedGame.Id, content.Id);
            Assert.Equal(nameof(GameStatusEnum.Scheduled), content.Status);
            Assert.Equal(finishedGame.Location, content.Location);
            Assert.Equal(finishedGame.Duration, content.Duration);
            Assert.Equal(finishedGame.TeamSize, content.TeamSize);
            Assert.Equal(ValidRequest.StartTime, content.StartTime);
            Assert.EndsWith($"/api/v1/games/{content.Id}", response.Headers.Location?.ToString(), StringComparison.OrdinalIgnoreCase);
        }

        private async Task<(User LinkedUser, Game Game)> SeedFinishedGameWithPlayersAsync(User organiser)
        {
            var linkedUser = EntityFactory.CreateUser(displayName: "Linked Player");
            linkedUser.ApplyRatingChange(123); // simulate a rating already updated by a previous RecordResult call

            var game = EntityFactory.CreateGame(
                organiser.Id, teamSize: 3, postCreationSteps: g => g.SetResult(GameTeamEnum.Home));

            var homePlayer = EntityFactory.CreatePlayer(
                game.Id, userId: linkedUser.Id, rating: 1000, type: PlayerTypeEnum.User, team: GameTeamEnum.Home);
            var awayPlayer = EntityFactory.CreatePlayer(
                game.Id, displayName: "Dummy Away Player", rating: 900, team: GameTeamEnum.Away,
                postCreationSteps: p => p.SetRatingChange(900, -20, 1));

            await using var scope = Factory.Services.CreateAsyncScope();
            var context = scope.ServiceProvider.GetRequiredService<ApiDbContext>();
            await context.Users.AddAsync(linkedUser, TestContext.Current.CancellationToken);
            await context.Games.AddAsync(game, TestContext.Current.CancellationToken);
            await context.Players.AddRangeAsync([homePlayer, awayPlayer], TestContext.Current.CancellationToken);
            await context.SaveChangesAsync(TestContext.Current.CancellationToken);

            return (linkedUser, game);
        }

        [Fact]
        public async Task ShouldCopyPlayers_UnassignedAndWithCurrentRatings()
        {
            var organiser = SeedOrganisers[0];
            var (linkedUser, game) = await SeedFinishedGameWithPlayersAsync(organiser);

            var request = CreateJsonRequest(HttpMethod.Post, $"{Url}/{game.Id}/copy", ValidRequest);
            WithActorHeaders(request, organiser);
            var response = await Client.SendAsync(request, TestContext.Current.CancellationToken);
            var copy = await ReadContentAsync<GameModel>(response, TestContext.Current.CancellationToken);
            Assert.NotNull(copy);

            var teamsRequest = CreateRequest(HttpMethod.Get, $"{Url}/{copy.Id}/teams");
            var teamsResponse = await Client.SendAsync(teamsRequest, TestContext.Current.CancellationToken);
            var teams = await ReadContentAsync<GameTeamsModel>(teamsResponse, TestContext.Current.CancellationToken);

            Assert.NotNull(teams);
            Assert.Empty(teams.Home!.Players);
            Assert.Empty(teams.Away!.Players);
            Assert.Equal(2, teams.Unassigned.Count);

            var copiedUserPlayer = teams.Unassigned.Single(p => p.Tag == linkedUser.Tag);
            Assert.Equal(linkedUser.Rating, copiedUserPlayer.Rating); // 1123 - the user's current (post-result) rating

            var copiedDummyPlayer = teams.Unassigned.Single(p => p.DisplayName == "Dummy Away Player");
            Assert.Equal(880, copiedDummyPlayer.Rating); // 900 + (-20) rating change from the source game
        }
    }
}