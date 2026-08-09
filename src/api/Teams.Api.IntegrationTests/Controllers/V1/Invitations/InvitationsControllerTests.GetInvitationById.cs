using System.Net;
using Teams.Api.Controllers.V1.Invitations.ResponseModels;
using Teams.Domain.Entities;

namespace Teams.Api.IntegrationTests.Controllers.V1.Invitations;

public static partial class InvitationsControllerTests
{
    public class GetInvitationById(ApiWebApplicationFactory factory) : InvitationsControllerTestsBase(factory)
    {
        [Fact]
        public async Task ShouldReturnBadRequest_WhenVersionIsUnsupported()
        {
            var existingInvitation = SeedInvitations[0];

            var request = CreateRequest(HttpMethod.Get, $"{VersionlessUrl}/{existingInvitation.Id}", apiVersion: "2.0");
            WithActorHeaders(request, Organiser);

            var response = await Client.SendAsync(request, TestContext.Current.CancellationToken);

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task ShouldReturnBadRequest_WhenVersionIsNotProvided()
        {
            var existingInvitation = SeedInvitations[0];

            var request = CreateRequest(HttpMethod.Get, $"{VersionlessUrl}/{existingInvitation.Id}", apiVersion: null);
            WithActorHeaders(request, Organiser);

            var response = await Client.SendAsync(request, TestContext.Current.CancellationToken);

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task ShouldReturnPreconditionRequired_WhenActorHeadersAreMissing()
        {
            var existingInvitation = SeedInvitations[0];

            var request = CreateRequest(HttpMethod.Get, $"{Url}/{existingInvitation.Id}");

            var response = await Client.SendAsync(request, TestContext.Current.CancellationToken);
            var problem = await ReadProblemDetailsAsync(response, TestContext.Current.CancellationToken);

            Assert.Equal(HttpStatusCode.PreconditionRequired, response.StatusCode);
            Assert.NotNull(problem);
            Assert.Equal("'Teams-User-Id' header value is required.", problem.Detail);
        }

        [Fact]
        public async Task ShouldReturnNotFound_WhenInvitationDoesNotExist()
        {
            const string id = "does-not-exist";

            var request = CreateRequest(HttpMethod.Get, $"{Url}/{id}");
            WithActorHeaders(request, Organiser);

            var response = await Client.SendAsync(request, TestContext.Current.CancellationToken);
            var problem = await ReadProblemDetailsAsync(response, TestContext.Current.CancellationToken);

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
            Assert.NotNull(problem);
            Assert.Equal(nameof(Invitation), problem.Extensions["resource"]?.ToString());
            Assert.Equal(id, problem.Extensions["identifier"]?.ToString());
        }

        [Fact]
        public async Task ShouldReturnForbidden_WhenActorIsNeitherOrganiserNorInvitedUser()
        {
            var existingInvitation = SeedInvitations[0];
            var bystander = SeedInvitees.First(u => u.Id != existingInvitation.UserId);

            var request = CreateRequest(HttpMethod.Get, $"{Url}/{existingInvitation.Id}");
            WithActorHeaders(request, bystander);

            var response = await Client.SendAsync(request, TestContext.Current.CancellationToken);
            var problem = await ReadProblemDetailsAsync(response, TestContext.Current.CancellationToken);

            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
            Assert.NotNull(problem);
            Assert.Equal("Action only available to game organiser or subject user.", problem.Detail);
        }

        [Fact]
        public async Task ShouldReturnOk_WhenActorIsOrganiser()
        {
            var existingInvitation = SeedInvitations[0];

            var request = CreateRequest(HttpMethod.Get, $"{Url}/{existingInvitation.Id}");
            WithActorHeaders(request, Organiser);

            var response = await Client.SendAsync(request, TestContext.Current.CancellationToken);
            var content = await ReadContentAsync<InvitationDetailModel>(response, TestContext.Current.CancellationToken);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.NotNull(content);
            Assert.Equal(existingInvitation.Id, content.Id);
            Assert.Equal(existingInvitation.Status.ToString(), content.Status);
            Assert.Equal(SeedGame.Id, content.Game.Id);
            Assert.Equal(Organiser.Id, content.Organiser.Id);
        }

        [Fact]
        public async Task ShouldReturnOk_WhenActorIsInvitedUser()
        {
            var existingInvitation = SeedInvitations[0];
            var invitee = SeedInvitees.Single(u => u.Id == existingInvitation.UserId);

            var request = CreateRequest(HttpMethod.Get, $"{Url}/{existingInvitation.Id}");
            WithActorHeaders(request, invitee);

            var response = await Client.SendAsync(request, TestContext.Current.CancellationToken);
            var content = await ReadContentAsync<InvitationDetailModel>(response, TestContext.Current.CancellationToken);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.NotNull(content);
            Assert.Equal(existingInvitation.Id, content.Id);
        }
    }
}