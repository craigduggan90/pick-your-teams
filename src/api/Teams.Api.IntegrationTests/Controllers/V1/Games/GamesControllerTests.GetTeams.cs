using Microsoft.Extensions.DependencyInjection;
using System.Net;
using Teams.Api.Controllers.V1.Games.ResponseModels;
using Teams.Data.Context;
using Teams.Domain.Entities;
using Teams.Domain.Enums;

namespace Teams.Api.IntegrationTests.Controllers.V1.Games;

public static partial class GamesControllerTests
{
    public class GetTeams(ApiWebApplicationFactory factory) : GamesControllerTestsBase(factory)
    {
        [Fact]
        public async Task ShouldReturnBadRequest_WhenVersionIsUnsupported()
        {
            var existingGame = SeedGames[0];

            var request = CreateRequest(HttpMethod.Get, $"{VersionlessUrl}/{existingGame.Id}/teams", apiVersion: "2.0");

            var response = await Client.SendAsync(request, TestContext.Current.CancellationToken);

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task ShouldReturnBadRequest_WhenVersionIsNotProvided()
        {
            var existingGame = SeedGames[0];

            var request = CreateRequest(HttpMethod.Get, $"{VersionlessUrl}/{existingGame.Id}/teams", apiVersion: null);

            var response = await Client.SendAsync(request, TestContext.Current.CancellationToken);

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task ShouldReturnNotFound_WhenGameDoesNotExist()
        {
            const string id = "does-not-exist";

            var request = CreateRequest(HttpMethod.Get, $"{Url}/{id}/teams");

            var response = await Client.SendAsync(request, TestContext.Current.CancellationToken);
            var problem = await ReadProblemDetailsAsync(response, TestContext.Current.CancellationToken);

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
            Assert.NotNull(problem);
            Assert.Equal(nameof(Game), problem.Extensions["resource"]?.ToString());
            Assert.Equal(id, problem.Extensions["identifier"]?.ToString());
        }

        [Fact]
        public async Task ShouldReturnOk_WithEmptyTeams_WhenNoPlayersAssigned()
        {
            var existingGame = SeedGames[0]; // no players attached in the shared seed

            var request = CreateRequest(HttpMethod.Get, $"{Url}/{existingGame.Id}/teams");

            var response = await Client.SendAsync(request, TestContext.Current.CancellationToken);
            var content = await ReadContentAsync<GameTeamsModel>(response, TestContext.Current.CancellationToken);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.NotNull(content);
            Assert.Empty(content.Home!.Players);
            Assert.Empty(content.Away!.Players);
        }

        [Fact]
        public async Task ShouldReturnOk_WithPlayersGroupedByTeam_WhenPlayersAreAssigned()
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

            var request = CreateRequest(HttpMethod.Get, $"{Url}/{game.Id}/teams");

            var response = await Client.SendAsync(request, TestContext.Current.CancellationToken);
            var content = await ReadContentAsync<GameTeamsModel>(response, TestContext.Current.CancellationToken);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.NotNull(content);
            Assert.Equal(game.Id, content.Id);
            Assert.Equal([homePlayer.Id], content.Home!.Players.Select(p => p.Id));
            Assert.Equal([awayPlayer.Id], content.Away!.Players.Select(p => p.Id));
        }
    }
}