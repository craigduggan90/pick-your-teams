using Microsoft.Extensions.DependencyInjection;
using System.Net;
using Teams.Api.Controllers.V1.Users.ResponseModels;
using Teams.Data.Context;
using Teams.Domain.Entities;

namespace Teams.Api.IntegrationTests.Controllers.V1.Users;

public static partial class UsersControllerTests
{
    public class GetSelf(ApiWebApplicationFactory factory) : UsersControllerTestsBase(factory)
    {
        private async Task<Invitation> SeedOpenInvitationAsync(User invitee, string organiserId)
        {
            await using var scope = Factory.Services.CreateAsyncScope();
            var context = scope.ServiceProvider.GetRequiredService<ApiDbContext>();

            var game = EntityFactory.CreateGame(organiserId);
            await context.Games.AddAsync(game, TestContext.Current.CancellationToken);

            var invitation = EntityFactory.CreateInvitation(game.Id, invitee.Id, invitee.EmailAddress);
            await context.Invitations.AddAsync(invitation, TestContext.Current.CancellationToken);

            await context.SaveChangesAsync(TestContext.Current.CancellationToken);
            return invitation;
        }

        [Fact]
        public async Task ShouldReturnBadRequest_WhenVersionIsUnsupported()
        {
            var existingUser = SeedUsers[0];

            var request = CreateRequest(HttpMethod.Get, $"{VersionlessUrl}/self", apiVersion: "2.0");
            WithActorHeaders(request, existingUser);

            var response = await Client.SendAsync(request, TestContext.Current.CancellationToken);

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task ShouldReturnBadRequest_WhenVersionIsNotProvided()
        {
            var existingUser = SeedUsers[0];

            var request = CreateRequest(HttpMethod.Get, $"{VersionlessUrl}/self", apiVersion: null);
            WithActorHeaders(request, existingUser);

            var response = await Client.SendAsync(request, TestContext.Current.CancellationToken);

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task ShouldReturnPreconditionRequired_WhenActorHeadersAreMissing()
        {
            var request = CreateRequest(HttpMethod.Get, $"{Url}/self");

            var response = await Client.SendAsync(request, TestContext.Current.CancellationToken);
            var problem = await ReadProblemDetailsAsync(response, TestContext.Current.CancellationToken);

            Assert.Equal(HttpStatusCode.PreconditionRequired, response.StatusCode);
            Assert.NotNull(problem);
            Assert.Equal("'Teams-User-Id' header value is required.", problem.Detail);
        }

        [Fact]
        public async Task ShouldReturnNotFound_WhenActorsUserDoesNotExist()
        {
            const string id = "does-not-exist";

            var request = CreateRequest(HttpMethod.Get, $"{Url}/self");
            WithActorHeaders(request, id, "does-not-exist-tag", "Ghost User");

            var response = await Client.SendAsync(request, TestContext.Current.CancellationToken);
            var problem = await ReadProblemDetailsAsync(response, TestContext.Current.CancellationToken);

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
            Assert.NotNull(problem);
            Assert.Equal(nameof(User), problem.Extensions["resource"]?.ToString());
            Assert.Equal(id, problem.Extensions["identifier"]?.ToString());
        }

        [Fact]
        public async Task ShouldReturnNotFound_WhenActorsUserIsDeleted()
        {
            var existingUser = SeedUsers[0];

            var deleteRequest = CreateRequest(HttpMethod.Delete, $"{Url}/{existingUser.Id}");
            WithActorHeaders(deleteRequest, existingUser);
            await Client.SendAsync(deleteRequest, TestContext.Current.CancellationToken);

            var request = CreateRequest(HttpMethod.Get, $"{Url}/self");
            WithActorHeaders(request, existingUser);

            var response = await Client.SendAsync(request, TestContext.Current.CancellationToken);
            var problem = await ReadProblemDetailsAsync(response, TestContext.Current.CancellationToken);

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
            Assert.NotNull(problem);
            Assert.Equal(existingUser.Id, problem.Extensions["identifier"]?.ToString());
        }

        [Fact]
        public async Task ShouldReturnOk_WithUserDetailContent_WhenUserExists()
        {
            var existingUser = SeedUsers[0];

            var request = CreateRequest(HttpMethod.Get, $"{Url}/self");
            WithActorHeaders(request, existingUser);

            var response = await Client.SendAsync(request, TestContext.Current.CancellationToken);
            var content = await ReadContentAsync<UserDetailModel>(response, TestContext.Current.CancellationToken);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.NotNull(content);
            Assert.Equal(existingUser.Id, content.Id);
            Assert.Equal(existingUser.Tag, content.Tag);
            Assert.Equal(existingUser.DisplayName, content.DisplayName);
            Assert.Equal(existingUser.Rating, content.Rating);
            Assert.Equal(existingUser.EmailAddress, content.Email);
            Assert.Equal(existingUser.Mobile, content.Mobile);
            Assert.Equal(0, content.PendingInvitations);
        }

        [Fact]
        public async Task ShouldReturnPendingInvitationsCount_WhenActorHasOpenInvitations()
        {
            var existingUser = SeedUsers[0];
            var organiser = SeedUsers[1];
            await SeedOpenInvitationAsync(existingUser, organiser.Id);
            await SeedOpenInvitationAsync(existingUser, organiser.Id);

            var request = CreateRequest(HttpMethod.Get, $"{Url}/self");
            WithActorHeaders(request, existingUser);

            var response = await Client.SendAsync(request, TestContext.Current.CancellationToken);
            var content = await ReadContentAsync<UserDetailModel>(response, TestContext.Current.CancellationToken);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.NotNull(content);
            Assert.Equal(2, content.PendingInvitations);
        }
    }
}