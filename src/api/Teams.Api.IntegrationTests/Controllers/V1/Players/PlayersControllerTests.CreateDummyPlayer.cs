using Microsoft.Extensions.DependencyInjection;
using System.Net;
using Teams.Api.Controllers.V1.Players.RequestModels;
using Teams.Api.Controllers.V1.Players.ResponseModel;
using Teams.Data.Context;
using Teams.Domain.Entities;
using Teams.Domain.Enums;

namespace Teams.Api.IntegrationTests.Controllers.V1.Players;

public static partial class PlayersControllerTests
{
    public class CreateDummyPlayer(ApiWebApplicationFactory factory) : PlayersControllerTestsBase(factory)
    {
        private CreateDummyPlayerRequestModel ValidRequest => new(SeedGame.Id, "Jess B", 1371);

        [Fact]
        public async Task ShouldReturnBadRequest_WhenVersionIsUnsupported()
        {
            var request = CreateJsonRequest(HttpMethod.Post, $"{VersionlessUrl}/dummy", ValidRequest, apiVersion: "2.0");
            WithActorHeaders(request, Organiser);

            var response = await Client.SendAsync(request, TestContext.Current.CancellationToken);

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task ShouldReturnBadRequest_WhenVersionIsNotProvided()
        {
            var request = CreateJsonRequest(HttpMethod.Post, $"{VersionlessUrl}/dummy", ValidRequest, apiVersion: null);
            WithActorHeaders(request, Organiser);

            var response = await Client.SendAsync(request, TestContext.Current.CancellationToken);

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task ShouldReturnUnprocessableEntity_WhenDisplayNameIsMissing()
        {
            var invalidRequest = ValidRequest with { DisplayName = "" };

            var request = CreateJsonRequest(HttpMethod.Post, $"{Url}/dummy", invalidRequest);
            WithActorHeaders(request, Organiser);

            var response = await Client.SendAsync(request, TestContext.Current.CancellationToken);
            var problem = await ReadProblemDetailsAsync(response, TestContext.Current.CancellationToken);

            Assert.Equal(HttpStatusCode.UnprocessableEntity, response.StatusCode);
            Assert.NotNull(problem);
            Assert.NotEmpty(GetValidationErrors(problem, nameof(CreateDummyPlayerRequestModel.DisplayName)));
        }

        [Fact]
        public async Task ShouldReturnUnprocessableEntity_WhenEstimatedRatingIsOutOfRange()
        {
            var invalidRequest = ValidRequest with { EstimatedRating = 2001 }; // above the maximum of 2000

            var request = CreateJsonRequest(HttpMethod.Post, $"{Url}/dummy", invalidRequest);
            WithActorHeaders(request, Organiser);

            var response = await Client.SendAsync(request, TestContext.Current.CancellationToken);
            var problem = await ReadProblemDetailsAsync(response, TestContext.Current.CancellationToken);

            Assert.Equal(HttpStatusCode.UnprocessableEntity, response.StatusCode);
            Assert.NotNull(problem);
            Assert.NotEmpty(GetValidationErrors(problem, nameof(CreateDummyPlayerRequestModel.EstimatedRating)));
        }

        [Fact]
        public async Task ShouldReturnNotFound_WhenGameDoesNotExist()
        {
            var invalidRequest = ValidRequest with { GameId = "does-not-exist" };

            var request = CreateJsonRequest(HttpMethod.Post, $"{Url}/dummy", invalidRequest);
            WithActorHeaders(request, Organiser);

            var response = await Client.SendAsync(request, TestContext.Current.CancellationToken);
            var problem = await ReadProblemDetailsAsync(response, TestContext.Current.CancellationToken);

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
            Assert.NotNull(problem);
            Assert.Equal(nameof(Game), problem.Extensions["resource"]?.ToString());
            Assert.Equal("does-not-exist", problem.Extensions["identifier"]?.ToString());
        }

        [Fact]
        public async Task ShouldReturnPreconditionRequired_WhenActorHeadersAreMissing()
        {
            var request = CreateJsonRequest(HttpMethod.Post, $"{Url}/dummy", ValidRequest);

            var response = await Client.SendAsync(request, TestContext.Current.CancellationToken);
            var problem = await ReadProblemDetailsAsync(response, TestContext.Current.CancellationToken);

            Assert.Equal(HttpStatusCode.PreconditionRequired, response.StatusCode);
            Assert.NotNull(problem);
            Assert.Equal("'Teams-User-Id' header value is required.", problem.Detail);
        }

        [Fact]
        public async Task ShouldReturnForbidden_WhenActorIsNotOrganiser()
        {
            var nonOrganiser = EntityFactory.CreateUser(displayName: "Non Organiser");

            await using (var scope = Factory.Services.CreateAsyncScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<ApiDbContext>();
                await context.Users.AddAsync(nonOrganiser, TestContext.Current.CancellationToken);
                await context.SaveChangesAsync(TestContext.Current.CancellationToken);
            }

            var request = CreateJsonRequest(HttpMethod.Post, $"{Url}/dummy", ValidRequest);
            WithActorHeaders(request, nonOrganiser);

            var response = await Client.SendAsync(request, TestContext.Current.CancellationToken);
            var problem = await ReadProblemDetailsAsync(response, TestContext.Current.CancellationToken);

            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
            Assert.NotNull(problem);
            Assert.Equal("Action only available to game organiser.", problem.Detail);
        }

        [Fact]
        public async Task ShouldReturnCreated_WithPlayerContent_WhenRequestIsValid()
        {
            var validRequest = ValidRequest;

            var request = CreateJsonRequest(HttpMethod.Post, $"{Url}/dummy", validRequest);
            WithActorHeaders(request, Organiser);

            var response = await Client.SendAsync(request, TestContext.Current.CancellationToken);
            var content = await ReadContentAsync<PlayerModel>(response, TestContext.Current.CancellationToken);

            Assert.Equal(HttpStatusCode.Created, response.StatusCode);
            Assert.NotNull(content);
            Assert.Equal(SeedGame.Id, content.GameId);
            Assert.Null(content.UserId);
            Assert.Equal(validRequest.DisplayName, content.DisplayName);
            Assert.Equal(validRequest.EstimatedRating, content.Rating);
            Assert.Equal(nameof(PlayerTypeEnum.Dummy), content.Type);
            Assert.EndsWith($"/api/v1/players/{content.Id}", response.Headers.Location?.ToString(), StringComparison.OrdinalIgnoreCase);
        }
    }
}