using Microsoft.Extensions.DependencyInjection;
using System.Net;
using Teams.Api.Controllers.V1.Games.ResponseModels;
using Teams.Data.Context;
using Teams.Domain.Entities;
using Teams.Domain.Enums;

namespace Teams.Api.IntegrationTests.Controllers.V1.Games;

public static partial class GamesControllerTests
{
    public class ClearTeams(ApiWebApplicationFactory factory) : GamesControllerTestsBase(factory)
    {
        [Fact]
        public async Task ShouldReturnBadRequest_WhenVersionIsUnsupported()
        {
            var existingGame = SeedGames[0];
            var organiser = SeedOrganisers.Single(u => u.Id == existingGame.OrganiserId);

            var request = CreateRequest(HttpMethod.Delete, $"{VersionlessUrl}/{existingGame.Id}/teams", apiVersion: "2.0");
            WithActorHeaders(request, organiser);

            var response = await Client.SendAsync(request, TestContext.Current.CancellationToken);

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task ShouldReturnBadRequest_WhenVersionIsNotProvided()
        {
            var existingGame = SeedGames[0];
            var organiser = SeedOrganisers.Single(u => u.Id == existingGame.OrganiserId);

            var request = CreateRequest(HttpMethod.Delete, $"{VersionlessUrl}/{existingGame.Id}/teams", apiVersion: null);
            WithActorHeaders(request, organiser);

            var response = await Client.SendAsync(request, TestContext.Current.CancellationToken);

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task ShouldReturnPreconditionRequired_WhenActorHeadersAreMissing()
        {
            var existingGame = SeedGames[0];

            var request = CreateRequest(HttpMethod.Delete, $"{Url}/{existingGame.Id}/teams");

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

            var request = CreateRequest(HttpMethod.Delete, $"{Url}/{existingGame.Id}/teams");
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

            var request = CreateRequest(HttpMethod.Delete, $"{Url}/{id}/teams");
            WithActorHeaders(request, SeedOrganisers[0]);

            var response = await Client.SendAsync(request, TestContext.Current.CancellationToken);
            var problem = await ReadProblemDetailsAsync(response, TestContext.Current.CancellationToken);

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
            Assert.NotNull(problem);
            Assert.Equal(nameof(Game), problem.Extensions["resource"]?.ToString());
            Assert.Equal(id, problem.Extensions["identifier"]?.ToString());
        }

        [Fact]
        public async Task ShouldReturnUnprocessableEntity_WhenGameIsFinished()
        {
            var finishedGame = SeedGames[1]; // seed game 2 - even index, already finished
            var organiser = SeedOrganisers.Single(u => u.Id == finishedGame.OrganiserId);

            var request = CreateRequest(HttpMethod.Delete, $"{Url}/{finishedGame.Id}/teams");
            WithActorHeaders(request, organiser);

            var response = await Client.SendAsync(request, TestContext.Current.CancellationToken);
            var problem = await ReadProblemDetailsAsync(response, TestContext.Current.CancellationToken);

            Assert.Equal(HttpStatusCode.UnprocessableEntity, response.StatusCode);
            Assert.NotNull(problem);
            Assert.Equal("Teams cannot be changed for a completed game.", problem.Detail);
        }

        [Fact]
        public async Task ShouldReturnNoContent_WithTeamsCleared_WhenRequestIsValid()
        {
            var organiser = SeedOrganisers[0];
            var game = EntityFactory.CreateGame(organiser.Id, teamSize: 3);
            var homePlayer = EntityFactory.CreatePlayer(game.Id, displayName: "Home Player", rating: 1000, team: GameTeamEnum.Home);
            var awayPlayer = EntityFactory.CreatePlayer(game.Id, displayName: "Away Player", rating: 950, team: GameTeamEnum.Away);

            await using (var scope = Factory.Services.CreateAsyncScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<ApiDbContext>();
                await context.Games.AddAsync(game, TestContext.Current.CancellationToken);
                await context.Players.AddRangeAsync([homePlayer, awayPlayer], TestContext.Current.CancellationToken);
                await context.SaveChangesAsync(TestContext.Current.CancellationToken);
            }

            var request = CreateRequest(HttpMethod.Delete, $"{Url}/{game.Id}/teams");
            WithActorHeaders(request, organiser);

            var response = await Client.SendAsync(request, TestContext.Current.CancellationToken);

            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

            var getRequest = CreateRequest(HttpMethod.Get, $"{Url}/{game.Id}/teams");
            var getResponse = await Client.SendAsync(getRequest, TestContext.Current.CancellationToken);
            var content = await ReadContentAsync<GameTeamsModel>(getResponse, TestContext.Current.CancellationToken);

            Assert.NotNull(content);
            Assert.Empty(content.Home!.Players);
            Assert.Empty(content.Away!.Players);
            Assert.Equal(
                new[] { homePlayer.Id, awayPlayer.Id }.OrderBy(id => id),
                content.Unassigned.Select(p => p.Id).OrderBy(id => id));
        }
    }
}