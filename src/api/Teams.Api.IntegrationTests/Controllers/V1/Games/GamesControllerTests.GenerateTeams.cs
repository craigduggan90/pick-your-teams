using Microsoft.Extensions.DependencyInjection;
using System.Net;
using Teams.Api.Controllers.V1.Games.RequestModels;
using Teams.Api.Controllers.V1.Games.ResponseModels;
using Teams.Data.Context;
using Teams.Domain.Entities;

namespace Teams.Api.IntegrationTests.Controllers.V1.Games;

public static partial class GamesControllerTests
{
    public class GenerateTeams(ApiWebApplicationFactory factory) : GamesControllerTestsBase(factory)
    {
        [Fact]
        public async Task ShouldReturnBadRequest_WhenVersionIsUnsupported()
        {
            var existingGame = SeedGames[0];

            var request = CreateJsonRequest(
                HttpMethod.Post, $"{VersionlessUrl}/{existingGame.Id}/teams/generate",
                new GenerateTeamsRequestModel([], [], 100), apiVersion: "2.0");

            var response = await Client.SendAsync(request, TestContext.Current.CancellationToken);

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task ShouldReturnBadRequest_WhenVersionIsNotProvided()
        {
            var existingGame = SeedGames[0];

            var request = CreateJsonRequest(
                HttpMethod.Post, $"{VersionlessUrl}/{existingGame.Id}/teams/generate",
                new GenerateTeamsRequestModel([], [], 100), apiVersion: null);

            var response = await Client.SendAsync(request, TestContext.Current.CancellationToken);

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task ShouldReturnNotFound_WhenGameDoesNotExist()
        {
            const string id = "does-not-exist";

            var request = CreateJsonRequest(HttpMethod.Post, $"{Url}/{id}/teams/generate", new GenerateTeamsRequestModel([], [], 100));

            var response = await Client.SendAsync(request, TestContext.Current.CancellationToken);
            var problem = await ReadProblemDetailsAsync(response, TestContext.Current.CancellationToken);

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
            Assert.NotNull(problem);
            Assert.Equal(nameof(Game), problem.Extensions["resource"]?.ToString());
            Assert.Equal(id, problem.Extensions["identifier"]?.ToString());
        }

        [Fact]
        public async Task ShouldReturnUnprocessableEntity_WhenDifferentialIsBelowMinimum()
        {
            var existingGame = SeedGames[0];
            var invalidRequest = new GenerateTeamsRequestModel([], [], 99); // below the minimum of 100

            var request = CreateJsonRequest(HttpMethod.Post, $"{Url}/{existingGame.Id}/teams/generate", invalidRequest);

            var response = await Client.SendAsync(request, TestContext.Current.CancellationToken);
            var problem = await ReadProblemDetailsAsync(response, TestContext.Current.CancellationToken);

            Assert.Equal(HttpStatusCode.UnprocessableEntity, response.StatusCode);
            Assert.NotNull(problem);
            Assert.NotEmpty(GetValidationErrors(problem, nameof(GenerateTeamsRequestModel.Differential)));
        }

        [Fact]
        public async Task ShouldReturnUnprocessableEntity_WhenGameHasFewerThanTwoPlayers()
        {
            var organiser = SeedOrganisers[0];
            var game = EntityFactory.CreateGame(organiser.Id, teamSize: 3);

            await using (var scope = Factory.Services.CreateAsyncScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<ApiDbContext>();
                await context.Games.AddAsync(game, TestContext.Current.CancellationToken);
                await context.SaveChangesAsync(TestContext.Current.CancellationToken);
            }

            var request = CreateJsonRequest(HttpMethod.Post, $"{Url}/{game.Id}/teams/generate", new GenerateTeamsRequestModel([], [], 100));

            var response = await Client.SendAsync(request, TestContext.Current.CancellationToken);

            Assert.Equal(HttpStatusCode.UnprocessableEntity, response.StatusCode);
        }

        [Fact]
        public async Task ShouldReturnOkWithNoBody_WhenNoSuggestionMeetsTheDifferentialThreshold()
        {
            var organiser = SeedOrganisers[0];
            var game = EntityFactory.CreateGame(organiser.Id, teamSize: 3);
            var strongPlayer = EntityFactory.CreatePlayer(game.Id, displayName: "Strong Player", rating: 2000);
            var weakPlayer = EntityFactory.CreatePlayer(game.Id, displayName: "Weak Player", rating: 100);

            await using (var scope = Factory.Services.CreateAsyncScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<ApiDbContext>();
                await context.Games.AddAsync(game, TestContext.Current.CancellationToken);
                await context.Players.AddRangeAsync([strongPlayer, weakPlayer], TestContext.Current.CancellationToken);
                await context.SaveChangesAsync(TestContext.Current.CancellationToken);
            }

            // Both players seeded, one per side - differential (1900) far exceeds the minimum threshold (100).
            var requestModel = new GenerateTeamsRequestModel([strongPlayer.Id], [weakPlayer.Id], 100);
            var request = CreateJsonRequest(HttpMethod.Post, $"{Url}/{game.Id}/teams/generate", requestModel);

            var response = await Client.SendAsync(request, TestContext.Current.CancellationToken);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Equal(0, response.Content.Headers.ContentLength ?? 0);
        }

        [Fact]
        public async Task ShouldReturnOkWithSuggestion_WhenRequestIsValid()
        {
            var organiser = SeedOrganisers[0];
            var game = EntityFactory.CreateGame(organiser.Id, teamSize: 3);
            var players = Enumerable.Range(1, 4)
                .Select(i => EntityFactory.CreatePlayer(game.Id, displayName: $"Player {i}", rating: 1000))
                .ToList();

            await using (var scope = Factory.Services.CreateAsyncScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<ApiDbContext>();
                await context.Games.AddAsync(game, TestContext.Current.CancellationToken);
                await context.Players.AddRangeAsync(players, TestContext.Current.CancellationToken);
                await context.SaveChangesAsync(TestContext.Current.CancellationToken);
            }

            var requestModel = new GenerateTeamsRequestModel([], [], 1000); // generous threshold, equal ratings
            var request = CreateJsonRequest(HttpMethod.Post, $"{Url}/{game.Id}/teams/generate", requestModel);

            var response = await Client.SendAsync(request, TestContext.Current.CancellationToken);
            var content = await ReadContentAsync<GameTeamsModel>(response, TestContext.Current.CancellationToken);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.NotNull(content);
            Assert.Equal(game.Id, content.Id);
            Assert.Equal(2, content.Home!.Players.Count);
            Assert.Equal(2, content.Away!.Players.Count);
            Assert.All(
                content.Home.Players.Concat(content.Away.Players),
                player => Assert.Contains(players, p => p.Id == player.Id));
            Assert.Empty(content.Unassigned);
        }
    }
}