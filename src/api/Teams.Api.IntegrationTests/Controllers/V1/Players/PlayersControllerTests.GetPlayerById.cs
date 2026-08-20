using System.Net;
using Teams.Api.Controllers.V1.Players.ResponseModel;
using Teams.Domain.Entities;

namespace Teams.Api.IntegrationTests.Controllers.V1.Players;

public static partial class PlayersControllerTests
{
    public class GetPlayerById(ApiWebApplicationFactory factory) : PlayersControllerTestsBase(factory)
    {
        [Fact]
        public async Task ShouldReturnBadRequest_WhenVersionIsUnsupported()
        {
            var existingPlayer = SeedPlayers[0];

            var request = CreateRequest(HttpMethod.Get, $"{VersionlessUrl}/{existingPlayer.Id}", apiVersion: "2.0");

            var response = await Client.SendAsync(request, TestContext.Current.CancellationToken);

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task ShouldReturnBadRequest_WhenVersionIsNotProvided()
        {
            var existingPlayer = SeedPlayers[0];

            var request = CreateRequest(HttpMethod.Get, $"{VersionlessUrl}/{existingPlayer.Id}", apiVersion: null);

            var response = await Client.SendAsync(request, TestContext.Current.CancellationToken);

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task ShouldReturnNotFound_WhenPlayerDoesNotExist()
        {
            const string id = "does-not-exist";

            var request = CreateRequest(HttpMethod.Get, $"{Url}/{id}");

            var response = await Client.SendAsync(request, TestContext.Current.CancellationToken);
            var problem = await ReadProblemDetailsAsync(response, TestContext.Current.CancellationToken);

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
            Assert.NotNull(problem);
            Assert.Equal(nameof(Player), problem.Extensions["resource"]?.ToString());
            Assert.Equal(id, problem.Extensions["identifier"]?.ToString());
        }

        [Fact]
        public async Task ShouldReturnOk_WithPlayerDetailContent_WhenPlayerExists()
        {
            var existingPlayer = SeedPlayers[0];

            var request = CreateRequest(HttpMethod.Get, $"{Url}/{existingPlayer.Id}");

            var response = await Client.SendAsync(request, TestContext.Current.CancellationToken);
            var content = await ReadContentAsync<PlayerDetailModel>(response, TestContext.Current.CancellationToken);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.NotNull(content);
            Assert.Equal(existingPlayer.Id, content.Id);
            Assert.Equal(existingPlayer.GameId, content.GameId);
            Assert.Equal(existingPlayer.GetDisplayName(), content.DisplayName);
            Assert.Equal(existingPlayer.Rating, content.Rating);
            Assert.Equal(existingPlayer.Team.ToString(), content.Team);
            Assert.Null(content.Tag); // seeded players are dummy players with no linked user
        }
    }
}