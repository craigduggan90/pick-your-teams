using Microsoft.Extensions.DependencyInjection;
using System.Net;
using Teams.Data.Context;
using Teams.Domain.Entities;
using Teams.Domain.Enums;

namespace Teams.Api.IntegrationTests.Controllers.V1.Players;

public static partial class PlayersControllerTests
{
    public class DeletePlayer(ApiWebApplicationFactory factory) : PlayersControllerTestsBase(factory)
    {
        private async Task<(User LinkedUser, Player Player)> SeedUserLinkedPlayerAsync()
        {
            var linkedUser = EntityFactory.CreateUser(displayName: "Linked Player");
            var player = EntityFactory.CreatePlayer(
                SeedGame.Id, userId: linkedUser.Id, displayName: linkedUser.Tag, rating: linkedUser.Rating, type: PlayerTypeEnum.User);

            await using var scope = Factory.Services.CreateAsyncScope();
            var context = scope.ServiceProvider.GetRequiredService<ApiDbContext>();
            await context.Users.AddAsync(linkedUser, TestContext.Current.CancellationToken);
            await context.Players.AddAsync(player, TestContext.Current.CancellationToken);
            await context.SaveChangesAsync(TestContext.Current.CancellationToken);

            return (linkedUser, player);
        }

        [Fact]
        public async Task ShouldReturnBadRequest_WhenVersionIsUnsupported()
        {
            var existingPlayer = SeedPlayers[0];

            var request = CreateRequest(HttpMethod.Delete, $"{VersionlessUrl}/{existingPlayer.Id}", apiVersion: "2.0");
            WithActorHeaders(request, Organiser);

            var response = await Client.SendAsync(request, TestContext.Current.CancellationToken);

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task ShouldReturnBadRequest_WhenVersionIsNotProvided()
        {
            var existingPlayer = SeedPlayers[0];

            var request = CreateRequest(HttpMethod.Delete, $"{VersionlessUrl}/{existingPlayer.Id}", apiVersion: null);
            WithActorHeaders(request, Organiser);

            var response = await Client.SendAsync(request, TestContext.Current.CancellationToken);

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task ShouldReturnPreconditionRequired_WhenActorHeadersAreMissing()
        {
            var existingPlayer = SeedPlayers[0];

            var request = CreateRequest(HttpMethod.Delete, $"{Url}/{existingPlayer.Id}");

            var response = await Client.SendAsync(request, TestContext.Current.CancellationToken);
            var problem = await ReadProblemDetailsAsync(response, TestContext.Current.CancellationToken);

            Assert.Equal(HttpStatusCode.PreconditionRequired, response.StatusCode);
            Assert.NotNull(problem);
            Assert.Equal("'Teams-User-Id' header value is required.", problem.Detail);
        }

        [Fact]
        public async Task ShouldReturnNotFound_WhenPlayerDoesNotExist()
        {
            const string id = "does-not-exist";

            var request = CreateRequest(HttpMethod.Delete, $"{Url}/{id}");
            WithActorHeaders(request, Organiser);

            var response = await Client.SendAsync(request, TestContext.Current.CancellationToken);
            var problem = await ReadProblemDetailsAsync(response, TestContext.Current.CancellationToken);

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
            Assert.NotNull(problem);
            Assert.Equal(nameof(Player), problem.Extensions["resource"]?.ToString());
            Assert.Equal(id, problem.Extensions["identifier"]?.ToString());
        }

        [Fact]
        public async Task ShouldReturnForbidden_WhenPlayerIsDummyAndActorIsNotOrganiser()
        {
            var dummyPlayer = SeedPlayers[0];
            var nonOrganiser = EntityFactory.CreateUser(displayName: "Non Organiser");

            await using (var scope = Factory.Services.CreateAsyncScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<ApiDbContext>();
                await context.Users.AddAsync(nonOrganiser, TestContext.Current.CancellationToken);
                await context.SaveChangesAsync(TestContext.Current.CancellationToken);
            }

            var request = CreateRequest(HttpMethod.Delete, $"{Url}/{dummyPlayer.Id}");
            WithActorHeaders(request, nonOrganiser);

            var response = await Client.SendAsync(request, TestContext.Current.CancellationToken);
            var problem = await ReadProblemDetailsAsync(response, TestContext.Current.CancellationToken);

            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
            Assert.NotNull(problem);
            Assert.Equal("Action only available to game organiser.", problem.Detail);
        }

        [Fact]
        public async Task ShouldReturnForbidden_WhenPlayerIsUserLinkedAndActorIsNeitherOrganiserNorTheLinkedUser()
        {
            var (_, player) = await SeedUserLinkedPlayerAsync();
            var bystander = EntityFactory.CreateUser(displayName: "Bystander");

            await using (var scope = Factory.Services.CreateAsyncScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<ApiDbContext>();
                await context.Users.AddAsync(bystander, TestContext.Current.CancellationToken);
                await context.SaveChangesAsync(TestContext.Current.CancellationToken);
            }

            var request = CreateRequest(HttpMethod.Delete, $"{Url}/{player.Id}");
            WithActorHeaders(request, bystander);

            var response = await Client.SendAsync(request, TestContext.Current.CancellationToken);
            var problem = await ReadProblemDetailsAsync(response, TestContext.Current.CancellationToken);

            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
            Assert.NotNull(problem);
            Assert.Equal("Action only available to game organiser or subject user.", problem.Detail);
        }

        [Fact]
        public async Task ShouldReturnNoContent_WithPlayerNoLongerRetrievable_WhenDummyPlayerDeletedByOrganiser()
        {
            var dummyPlayer = SeedPlayers[0];

            var request = CreateRequest(HttpMethod.Delete, $"{Url}/{dummyPlayer.Id}");
            WithActorHeaders(request, Organiser);

            var response = await Client.SendAsync(request, TestContext.Current.CancellationToken);

            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

            var getRequest = CreateRequest(HttpMethod.Get, $"{Url}/{dummyPlayer.Id}");
            var getResponse = await Client.SendAsync(getRequest, TestContext.Current.CancellationToken);

            Assert.Equal(HttpStatusCode.NotFound, getResponse.StatusCode);
        }

        [Fact]
        public async Task ShouldReturnNoContent_WhenUserLinkedPlayerDeletedByTheLinkedUserThemselves()
        {
            var (linkedUser, player) = await SeedUserLinkedPlayerAsync();

            var request = CreateRequest(HttpMethod.Delete, $"{Url}/{player.Id}");
            WithActorHeaders(request, linkedUser); // self, not organiser

            var response = await Client.SendAsync(request, TestContext.Current.CancellationToken);

            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

            var getRequest = CreateRequest(HttpMethod.Get, $"{Url}/{player.Id}");
            var getResponse = await Client.SendAsync(getRequest, TestContext.Current.CancellationToken);

            Assert.Equal(HttpStatusCode.NotFound, getResponse.StatusCode);
        }
    }
}