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
    public class CreatePlayer(ApiWebApplicationFactory factory) : PlayersControllerTestsBase(factory)
    {
        private async Task<User> SeedUserAsync(string displayName = "New Player")
        {
            var user = EntityFactory.CreateUser(displayName: displayName);
            await using var scope = Factory.Services.CreateAsyncScope();
            var context = scope.ServiceProvider.GetRequiredService<ApiDbContext>();
            await context.Users.AddAsync(user, TestContext.Current.CancellationToken);
            await context.SaveChangesAsync(TestContext.Current.CancellationToken);
            return user;
        }

        [Fact]
        public async Task ShouldReturnBadRequest_WhenVersionIsUnsupported()
        {
            var request = CreateJsonRequest(
                HttpMethod.Post, VersionlessUrl, new CreatePlayerRequestModel(SeedGame.Id, Organiser.Id), apiVersion: "2.0");
            WithActorHeaders(request, Organiser);

            var response = await Client.SendAsync(request, TestContext.Current.CancellationToken);

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task ShouldReturnBadRequest_WhenVersionIsNotProvided()
        {
            var request = CreateJsonRequest(
                HttpMethod.Post, VersionlessUrl, new CreatePlayerRequestModel(SeedGame.Id, Organiser.Id), apiVersion: null);
            WithActorHeaders(request, Organiser);

            var response = await Client.SendAsync(request, TestContext.Current.CancellationToken);

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task ShouldReturnPreconditionRequired_WhenActorHeadersAreMissing()
        {
            var request = CreateJsonRequest(HttpMethod.Post, Url, new CreatePlayerRequestModel(SeedGame.Id, Organiser.Id));

            var response = await Client.SendAsync(request, TestContext.Current.CancellationToken);
            var problem = await ReadProblemDetailsAsync(response, TestContext.Current.CancellationToken);

            Assert.Equal(HttpStatusCode.PreconditionRequired, response.StatusCode);
            Assert.NotNull(problem);
            Assert.Equal("'Teams-User-Id' header value is required.", problem.Detail);
        }

        [Fact]
        public async Task ShouldReturnForbidden_WhenActorIsNeitherOrganiserNorTargetUser()
        {
            var newUser = await SeedUserAsync();
            var bystander = await SeedUserAsync("Bystander");

            var request = CreateJsonRequest(HttpMethod.Post, Url, new CreatePlayerRequestModel(SeedGame.Id, newUser.Id));
            WithActorHeaders(request, bystander);

            var response = await Client.SendAsync(request, TestContext.Current.CancellationToken);
            var problem = await ReadProblemDetailsAsync(response, TestContext.Current.CancellationToken);

            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
            Assert.NotNull(problem);
            Assert.Equal("Action only available to game organiser or subject user.", problem.Detail);
        }

        [Fact]
        public async Task ShouldReturnNotFound_WhenGameDoesNotExist()
        {
            var newUser = await SeedUserAsync();

            var request = CreateJsonRequest(HttpMethod.Post, Url, new CreatePlayerRequestModel("does-not-exist", newUser.Id));
            WithActorHeaders(request, newUser);

            var response = await Client.SendAsync(request, TestContext.Current.CancellationToken);
            var problem = await ReadProblemDetailsAsync(response, TestContext.Current.CancellationToken);

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
            Assert.NotNull(problem);
            Assert.Equal(nameof(Game), problem.Extensions["resource"]?.ToString());
        }

        [Fact]
        public async Task ShouldReturnNotFound_WhenUserDoesNotExist()
        {
            var request = CreateJsonRequest(HttpMethod.Post, Url, new CreatePlayerRequestModel(SeedGame.Id, "does-not-exist"));
            WithActorHeaders(request, Organiser); // organiser branch avoids needing actor id to equal the missing user

            var response = await Client.SendAsync(request, TestContext.Current.CancellationToken);
            var problem = await ReadProblemDetailsAsync(response, TestContext.Current.CancellationToken);

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
            Assert.NotNull(problem);
            Assert.Equal(nameof(User), problem.Extensions["resource"]?.ToString());
            Assert.Equal("does-not-exist", problem.Extensions["identifier"]?.ToString());
        }

        [Fact]
        public async Task ShouldReturnUnprocessableEntity_WhenUserAlreadyHasAPlayerOnTheGame()
        {
            var existingUser = await SeedUserAsync();
            var existingPlayer = EntityFactory.CreatePlayer(
                SeedGame.Id, userId: existingUser.Id, displayName: existingUser.Tag, rating: existingUser.Rating, type: PlayerTypeEnum.User);

            await using (var scope = Factory.Services.CreateAsyncScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<ApiDbContext>();
                await context.Players.AddAsync(existingPlayer, TestContext.Current.CancellationToken);
                await context.SaveChangesAsync(TestContext.Current.CancellationToken);
            }

            var request = CreateJsonRequest(HttpMethod.Post, Url, new CreatePlayerRequestModel(SeedGame.Id, existingUser.Id));
            WithActorHeaders(request, existingUser);

            var response = await Client.SendAsync(request, TestContext.Current.CancellationToken);
            var problem = await ReadProblemDetailsAsync(response, TestContext.Current.CancellationToken);

            Assert.Equal(HttpStatusCode.UnprocessableEntity, response.StatusCode);
            Assert.NotNull(problem);
            Assert.Contains(
                "User is already associated with game.",
                GetValidationErrors(problem, nameof(CreatePlayerRequestModel.UserId)));
        }

        [Fact]
        public async Task ShouldReturnCreated_WithPlayerContent_WhenActorIsTheTargetUser()
        {
            var newUser = await SeedUserAsync();

            var request = CreateJsonRequest(HttpMethod.Post, Url, new CreatePlayerRequestModel(SeedGame.Id, newUser.Id));
            WithActorHeaders(request, newUser);

            var response = await Client.SendAsync(request, TestContext.Current.CancellationToken);
            var content = await ReadContentAsync<PlayerModel>(response, TestContext.Current.CancellationToken);

            Assert.Equal(HttpStatusCode.Created, response.StatusCode);
            Assert.NotNull(content);
            Assert.Equal(SeedGame.Id, content.GameId);
            Assert.Equal(newUser.Id, content.UserId);
            Assert.Equal(newUser.Tag, content.DisplayName);
            Assert.Equal(newUser.Rating, content.Rating);
            Assert.Equal(nameof(PlayerTypeEnum.User), content.Type);
            Assert.EndsWith($"/api/v1/players/{content.Id}", response.Headers.Location?.ToString(), StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public async Task ShouldReturnCreated_WhenActorIsTheOrganiserAddingSomeoneElse()
        {
            var newUser = await SeedUserAsync();

            var request = CreateJsonRequest(HttpMethod.Post, Url, new CreatePlayerRequestModel(SeedGame.Id, newUser.Id));
            WithActorHeaders(request, Organiser);

            var response = await Client.SendAsync(request, TestContext.Current.CancellationToken);
            var content = await ReadContentAsync<PlayerModel>(response, TestContext.Current.CancellationToken);

            Assert.Equal(HttpStatusCode.Created, response.StatusCode);
            Assert.NotNull(content);
            Assert.Equal(newUser.Id, content.UserId);
        }
    }
}