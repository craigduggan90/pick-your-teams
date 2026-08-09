using System.Net;
using Teams.Api.Controllers.V1.Games.RequestModels;
using Teams.Api.Controllers.V1.Games.ResponseModels;
using Teams.Domain.Entities;

namespace Teams.Api.IntegrationTests.Controllers.V1.Games;

public static partial class GamesControllerTests
{
    public class UpdateGame(ApiWebApplicationFactory factory) : GamesControllerTestsBase(factory)
    {
        private static UpdateGameRequestModel ValidRequest =>
            new(Location: "Updated Venue", StartTime: null, Duration: null);

        [Fact]
        public async Task ShouldReturnBadRequest_WhenVersionIsUnsupported()
        {
            var existingGame = SeedGames[0];
            var organiser = SeedOrganisers.Single(u => u.Id == existingGame.OrganiserId);

            var request = CreateJsonRequest(
                HttpMethod.Patch,
                $"{VersionlessUrl}/{existingGame.Id}",
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
                HttpMethod.Patch,
                $"{VersionlessUrl}/{existingGame.Id}",
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

            var request = CreateJsonRequest(HttpMethod.Patch, $"{Url}/{existingGame.Id}", ValidRequest);

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

            var request = CreateJsonRequest(HttpMethod.Patch, $"{Url}/{existingGame.Id}", ValidRequest);
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

            var request = CreateJsonRequest(HttpMethod.Patch, $"{Url}/{id}", ValidRequest);
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

            var request = CreateJsonRequest(HttpMethod.Patch, $"{Url}/{finishedGame.Id}", ValidRequest);
            WithActorHeaders(request, organiser);

            var response = await Client.SendAsync(request, TestContext.Current.CancellationToken);
            var problem = await ReadProblemDetailsAsync(response, TestContext.Current.CancellationToken);

            Assert.Equal(HttpStatusCode.UnprocessableEntity, response.StatusCode);
            Assert.NotNull(problem);
            Assert.Equal("Game cannot be updated once finished.", problem.Detail);
        }

        [Fact]
        public async Task ShouldReturnUnprocessableEntity_WhenDurationIsOutOfRange()
        {
            var existingGame = SeedGames[0];
            var organiser = SeedOrganisers.Single(u => u.Id == existingGame.OrganiserId);
            var invalidRequest = ValidRequest with { Duration = 200 }; // above the maximum of 120

            var request = CreateJsonRequest(HttpMethod.Patch, $"{Url}/{existingGame.Id}", invalidRequest);
            WithActorHeaders(request, organiser);

            var response = await Client.SendAsync(request, TestContext.Current.CancellationToken);
            var problem = await ReadProblemDetailsAsync(response, TestContext.Current.CancellationToken);

            Assert.Equal(HttpStatusCode.UnprocessableEntity, response.StatusCode);
            Assert.NotNull(problem);
            Assert.NotEmpty(GetValidationErrors(problem, nameof(UpdateGameRequestModel.Duration)));
        }

        [Fact]
        public async Task ShouldReturnNoContent_WithPersistedChanges_WhenRequestIsValid()
        {
            var existingGame = SeedGames[0];
            var organiser = SeedOrganisers.Single(u => u.Id == existingGame.OrganiserId);
            var updatedRequest = ValidRequest with { Location = "Brand New Venue" };

            var request = CreateJsonRequest(HttpMethod.Patch, $"{Url}/{existingGame.Id}", updatedRequest);
            WithActorHeaders(request, organiser);

            var response = await Client.SendAsync(request, TestContext.Current.CancellationToken);

            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

            var getRequest = CreateRequest(HttpMethod.Get, $"{Url}/{existingGame.Id}");
            var getResponse = await Client.SendAsync(getRequest, TestContext.Current.CancellationToken);
            var detail = await ReadContentAsync<GameDetailModel>(getResponse, TestContext.Current.CancellationToken);

            Assert.NotNull(detail);
            Assert.Equal(updatedRequest.Location, detail.Location);
        }
    }
}