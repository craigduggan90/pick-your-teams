using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Net;
using Teams.Api.Controllers.V1.Users.ResponseModels;
using Teams.Data.Context;
using Teams.Domain.Entities;
using Teams.Domain.Enums;

namespace Teams.Api.IntegrationTests.Controllers.V1.Games;

public static partial class GamesControllerTests
{
    public class DeleteGame(ApiWebApplicationFactory factory) : GamesControllerTestsBase(factory)
    {
        private async Task<Invitation> SeedOpenInvitationAsync(string gameId, User invitee)
        {
            await using var scope = Factory.Services.CreateAsyncScope();
            var context = scope.ServiceProvider.GetRequiredService<ApiDbContext>();

            var invitation = EntityFactory.CreateInvitation(gameId, invitee.Id, invitee.EmailAddress);
            await context.Invitations.AddAsync(invitation, TestContext.Current.CancellationToken);
            await context.SaveChangesAsync(TestContext.Current.CancellationToken);

            return invitation;
        }

        private async Task<Invitation> GetInvitationDirectlyAsync(string id)
        {
            await using var scope = Factory.Services.CreateAsyncScope();
            var context = scope.ServiceProvider.GetRequiredService<ApiDbContext>();

            // IgnoreQueryFilters: the point of this fetch is to check the invitation's own Status
            // after its game is soft-deleted - going through the normal repository would apply
            // Game's query filter to the Include and silently 404 an invitation that still exists.
            return await context.Invitations.IgnoreQueryFilters().SingleAsync(
                entity => entity.Id == id, TestContext.Current.CancellationToken);
        }

        [Fact]
        public async Task ShouldReturnBadRequest_WhenVersionIsUnsupported()
        {
            var existingGame = SeedGames[0];
            var organiser = SeedOrganisers.Single(u => u.Id == existingGame.OrganiserId);

            var request = CreateRequest(HttpMethod.Delete, $"{VersionlessUrl}/{existingGame.Id}", apiVersion: "2.0");
            WithActorHeaders(request, organiser);

            var response = await Client.SendAsync(request, TestContext.Current.CancellationToken);

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task ShouldReturnBadRequest_WhenVersionIsNotProvided()
        {
            var existingGame = SeedGames[0];
            var organiser = SeedOrganisers.Single(u => u.Id == existingGame.OrganiserId);

            var request = CreateRequest(HttpMethod.Delete, $"{VersionlessUrl}/{existingGame.Id}", apiVersion: null);
            WithActorHeaders(request, organiser);

            var response = await Client.SendAsync(request, TestContext.Current.CancellationToken);

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task ShouldReturnPreconditionRequired_WhenActorHeadersAreMissing()
        {
            var existingGame = SeedGames[0];

            var request = CreateRequest(HttpMethod.Delete, $"{Url}/{existingGame.Id}");

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

            var request = CreateRequest(HttpMethod.Delete, $"{Url}/{existingGame.Id}");
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

            var request = CreateRequest(HttpMethod.Delete, $"{Url}/{id}");
            WithActorHeaders(request, SeedOrganisers[0]);

            var response = await Client.SendAsync(request, TestContext.Current.CancellationToken);
            var problem = await ReadProblemDetailsAsync(response, TestContext.Current.CancellationToken);

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
            Assert.NotNull(problem);
            Assert.Equal(nameof(Game), problem.Extensions["resource"]?.ToString());
            Assert.Equal(id, problem.Extensions["identifier"]?.ToString());
        }

        [Fact]
        public async Task ShouldReturnNotFound_WhenGameIsAlreadyDeleted()
        {
            var existingGame = SeedGames[0];
            var organiser = SeedOrganisers.Single(u => u.Id == existingGame.OrganiserId);

            var deleteRequest = CreateRequest(HttpMethod.Delete, $"{Url}/{existingGame.Id}");
            WithActorHeaders(deleteRequest, organiser);
            await Client.SendAsync(deleteRequest, TestContext.Current.CancellationToken);

            var secondDeleteRequest = CreateRequest(HttpMethod.Delete, $"{Url}/{existingGame.Id}");
            WithActorHeaders(secondDeleteRequest, organiser);

            var response = await Client.SendAsync(secondDeleteRequest, TestContext.Current.CancellationToken);
            var problem = await ReadProblemDetailsAsync(response, TestContext.Current.CancellationToken);

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
            Assert.NotNull(problem);
            Assert.Equal(existingGame.Id, problem.Extensions["identifier"]?.ToString());
        }

        [Fact]
        public async Task ShouldReturnNoContent_WithGameNoLongerRetrievable_WhenRequestIsValid()
        {
            var existingGame = SeedGames[0];
            var organiser = SeedOrganisers.Single(u => u.Id == existingGame.OrganiserId);

            var request = CreateRequest(HttpMethod.Delete, $"{Url}/{existingGame.Id}");
            WithActorHeaders(request, organiser);

            var response = await Client.SendAsync(request, TestContext.Current.CancellationToken);

            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

            var getRequest = CreateRequest(HttpMethod.Get, $"{Url}/{existingGame.Id}");
            var getResponse = await Client.SendAsync(getRequest, TestContext.Current.CancellationToken);

            Assert.Equal(HttpStatusCode.NotFound, getResponse.StatusCode);
        }

        [Fact]
        public async Task ShouldFailOpenInvitationsForTheGame_SoThePendingInvitationsCountDropsAfterDelete()
        {
            var existingGame = SeedGames[0];
            var organiser = SeedOrganisers.Single(u => u.Id == existingGame.OrganiserId);
            var invitee = SeedOrganisers.First(u => u.Id != existingGame.OrganiserId);
            var invitation = await SeedOpenInvitationAsync(existingGame.Id, invitee);

            var selfRequestBefore = CreateRequest(HttpMethod.Get, "api/v1/users/self");
            WithActorHeaders(selfRequestBefore, invitee);
            var selfResponseBefore = await Client.SendAsync(selfRequestBefore, TestContext.Current.CancellationToken);
            var selfContentBefore = await ReadContentAsync<UserDetailModel>(selfResponseBefore, TestContext.Current.CancellationToken);
            Assert.Equal(1, selfContentBefore!.PendingInvitations);

            var deleteRequest = CreateRequest(HttpMethod.Delete, $"{Url}/{existingGame.Id}");
            WithActorHeaders(deleteRequest, organiser);
            var deleteResponse = await Client.SendAsync(deleteRequest, TestContext.Current.CancellationToken);
            Assert.Equal(HttpStatusCode.NoContent, deleteResponse.StatusCode);

            var selfRequestAfter = CreateRequest(HttpMethod.Get, "api/v1/users/self");
            WithActorHeaders(selfRequestAfter, invitee);
            var selfResponseAfter = await Client.SendAsync(selfRequestAfter, TestContext.Current.CancellationToken);
            var selfContentAfter = await ReadContentAsync<UserDetailModel>(selfResponseAfter, TestContext.Current.CancellationToken);
            Assert.Equal(0, selfContentAfter!.PendingInvitations);

            var invitationAfter = await GetInvitationDirectlyAsync(invitation.Id);
            Assert.Equal(InvitationStatusEnum.Failed, invitationAfter.Status);
            Assert.Equal("Game was deleted.", invitationAfter.ErrorMessage);
        }
    }
}