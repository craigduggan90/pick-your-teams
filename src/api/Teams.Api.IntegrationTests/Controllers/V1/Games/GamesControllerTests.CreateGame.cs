using System.Net;
using Teams.Api.Controllers.V1.Games.RequestModels;
using Teams.Api.Controllers.V1.Games.ResponseModels;

namespace Teams.Api.IntegrationTests.Controllers.V1.Games;

public static partial class GamesControllerTests
{
    public class CreateGame(ApiWebApplicationFactory factory) : GamesControllerTestsBase(factory)
    {
        private CreateGameRequestModel ValidRequest => new(
            Location: "Oak Leaf Leisure Centre",
            StartTime: new DateTime(2026, 7, 31, 20, 45, 0, DateTimeKind.Utc),
            Duration: 60,
            TeamSize: 5,
            OrganiserId: SeedOrganisers[0].Id);

        [Fact]
        public async Task ShouldReturnBadRequest_WhenVersionIsUnsupported()
        {
            var request = CreateJsonRequest(HttpMethod.Post, VersionlessUrl, ValidRequest, apiVersion: "2.0");

            var response = await Client.SendAsync(request, TestContext.Current.CancellationToken);

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task ShouldReturnBadRequest_WhenVersionIsNotProvided()
        {
            var request = CreateJsonRequest(HttpMethod.Post, VersionlessUrl, ValidRequest, apiVersion: null);

            var response = await Client.SendAsync(request, TestContext.Current.CancellationToken);

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task ShouldReturnUnprocessableEntity_WhenTeamSizeIsOutOfRange()
        {
            var invalidRequest = ValidRequest with { TeamSize = 2 }; // below the minimum of 3

            var request = CreateJsonRequest(HttpMethod.Post, Url, invalidRequest);

            var response = await Client.SendAsync(request, TestContext.Current.CancellationToken);
            var problem = await ReadProblemDetailsAsync(response, TestContext.Current.CancellationToken);

            Assert.Equal(HttpStatusCode.UnprocessableEntity, response.StatusCode);
            Assert.NotNull(problem);
            Assert.NotEmpty(GetValidationErrors(problem, nameof(CreateGameRequestModel.TeamSize)));
        }

        [Fact]
        public async Task ShouldReturnUnprocessableEntity_WhenDurationIsOutOfRange()
        {
            var invalidRequest = ValidRequest with { Duration = 10 }; // below the minimum of 15

            var request = CreateJsonRequest(HttpMethod.Post, Url, invalidRequest);

            var response = await Client.SendAsync(request, TestContext.Current.CancellationToken);
            var problem = await ReadProblemDetailsAsync(response, TestContext.Current.CancellationToken);

            Assert.Equal(HttpStatusCode.UnprocessableEntity, response.StatusCode);
            Assert.NotNull(problem);
            Assert.NotEmpty(GetValidationErrors(problem, nameof(CreateGameRequestModel.Duration)));
        }

        [Fact]
        public async Task ShouldReturnNotFound_WhenOrganiserDoesNotExist()
        {
            var invalidRequest = ValidRequest with { OrganiserId = "does-not-exist" };

            var request = CreateJsonRequest(HttpMethod.Post, Url, invalidRequest);

            var response = await Client.SendAsync(request, TestContext.Current.CancellationToken);
            var problem = await ReadProblemDetailsAsync(response, TestContext.Current.CancellationToken);

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
            Assert.NotNull(problem);
            Assert.Equal("does-not-exist", problem.Extensions["identifier"]?.ToString());
        }

        [Fact]
        public async Task ShouldReturnCreated_WithGameContent_WhenRequestIsValid()
        {
            var validRequest = ValidRequest;

            var request = CreateJsonRequest(HttpMethod.Post, Url, validRequest);

            var response = await Client.SendAsync(request, TestContext.Current.CancellationToken);
            var content = await ReadContentAsync<GameModel>(response, TestContext.Current.CancellationToken);

            Assert.Equal(HttpStatusCode.Created, response.StatusCode);
            Assert.NotNull(content);
            Assert.Equal(validRequest.Location, content.Location);
            Assert.Equal(validRequest.StartTime, content.StartTime);
            Assert.Equal(validRequest.Duration, content.Duration);
            Assert.Equal(validRequest.TeamSize, content.TeamSize);
            Assert.EndsWith($"/api/v1/games/{content.Id}", response.Headers.Location?.ToString(), StringComparison.OrdinalIgnoreCase);
        }
    }
}