using System.Net;
using Teams.Api.Controllers.V1.Games.ResponseModels;
using Teams.Domain.Entities;

namespace Teams.Api.IntegrationTests.Controllers.V1.Games;

public static partial class GamesControllerTests
{
    public class GetGameById(ApiWebApplicationFactory factory) : GamesControllerTestsBase(factory)
    {
        [Fact]
        public async Task ShouldReturnBadRequest_WhenVersionIsUnsupported()
        {
            var existingGame = SeedGames[0];

            var request = CreateRequest(HttpMethod.Get, $"{VersionlessUrl}/{existingGame.Id}", apiVersion: "2.0");

            var response = await Client.SendAsync(request, TestContext.Current.CancellationToken);

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task ShouldReturnBadRequest_WhenVersionIsNotProvided()
        {
            var existingGame = SeedGames[0];

            var request = CreateRequest(HttpMethod.Get, $"{VersionlessUrl}/{existingGame.Id}", apiVersion: null);

            var response = await Client.SendAsync(request, TestContext.Current.CancellationToken);

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task ShouldReturnNotFound_WhenGameDoesNotExist()
        {
            const string id = "does-not-exist";

            var request = CreateRequest(HttpMethod.Get, $"{Url}/{id}");

            var response = await Client.SendAsync(request, TestContext.Current.CancellationToken);
            var problem = await ReadProblemDetailsAsync(response, TestContext.Current.CancellationToken);

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
            Assert.NotNull(problem);
            Assert.Equal(nameof(Game), problem.Extensions["resource"]?.ToString());
            Assert.Equal(id, problem.Extensions["identifier"]?.ToString());
        }

        [Fact]
        public async Task ShouldReturnOk_WithGameDetailContent_WhenGameExists()
        {
            var existingGame = SeedGames[0];

            var request = CreateRequest(HttpMethod.Get, $"{Url}/{existingGame.Id}");

            var response = await Client.SendAsync(request, TestContext.Current.CancellationToken);
            var content = await ReadContentAsync<GameDetailModel>(response, TestContext.Current.CancellationToken);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.NotNull(content);
            Assert.Equal(existingGame.Id, content.Id);
            Assert.Equal(existingGame.Location, content.Location);
            Assert.Equal(existingGame.StartTime, content.StartTime);
            Assert.Equal(existingGame.Duration, content.Duration);
            Assert.Equal(existingGame.TeamSize, content.TeamSize);
            Assert.Equal(existingGame.Status.ToString(), content.Status);

            var organiser = SeedOrganisers.Single(o => o.Id == existingGame.OrganiserId);
            Assert.Equal(organiser.Id, content.Organiser!.Id);
            Assert.Equal(organiser.Tag, content.Organiser.Tag);
            Assert.Equal(organiser.DisplayName, content.Organiser.DisplayName);
        }

        [Fact]
        public async Task ShouldReturnOk_WithWinnerAndRatings_WhenGameIsFinished()
        {
            var finishedGame = SeedGames[1]; // seed game 2 - even index, finished

            var request = CreateRequest(HttpMethod.Get, $"{Url}/{finishedGame.Id}");

            var response = await Client.SendAsync(request, TestContext.Current.CancellationToken);
            var content = await ReadContentAsync<GameDetailModel>(response, TestContext.Current.CancellationToken);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.NotNull(content);
            Assert.Equal(finishedGame.Winner?.ToString(), content.Winner);
            Assert.Equal(finishedGame.HomeTeamRating, content.HomeTeamRating);
            Assert.Equal(finishedGame.AwayTeamRating, content.AwayTeamRating);
        }
    }
}