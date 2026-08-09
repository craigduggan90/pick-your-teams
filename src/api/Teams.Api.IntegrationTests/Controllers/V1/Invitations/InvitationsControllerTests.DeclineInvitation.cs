using System.Net;
using Teams.Api.Controllers.V1.Invitations.ResponseModels;
using Teams.Domain.Entities;
using Teams.Domain.Enums;

namespace Teams.Api.IntegrationTests.Controllers.V1.Invitations;

public static partial class InvitationsControllerTests
{
    public class DeclineInvitation(ApiWebApplicationFactory factory) : InvitationsControllerTestsBase(factory)
    {
        [Fact]
        public async Task ShouldReturnBadRequest_WhenVersionIsUnsupported()
        {
            var existingInvitation = SeedInvitations.First(i => i.Status == InvitationStatusEnum.Open);
            var invitee = SeedInvitees.Single(u => u.Id == existingInvitation.UserId);

            var request = CreateRequest(HttpMethod.Delete, $"{VersionlessUrl}/{existingInvitation.Id}", apiVersion: "2.0");
            WithActorHeaders(request, invitee);

            var response = await Client.SendAsync(request, TestContext.Current.CancellationToken);

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task ShouldReturnBadRequest_WhenVersionIsNotProvided()
        {
            var existingInvitation = SeedInvitations.First(i => i.Status == InvitationStatusEnum.Open);
            var invitee = SeedInvitees.Single(u => u.Id == existingInvitation.UserId);

            var request = CreateRequest(HttpMethod.Delete, $"{VersionlessUrl}/{existingInvitation.Id}", apiVersion: null);
            WithActorHeaders(request, invitee);

            var response = await Client.SendAsync(request, TestContext.Current.CancellationToken);

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task ShouldReturnPreconditionRequired_WhenActorHeadersAreMissing()
        {
            var existingInvitation = SeedInvitations.First(i => i.Status == InvitationStatusEnum.Open);

            var request = CreateRequest(HttpMethod.Delete, $"{Url}/{existingInvitation.Id}");

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

            var request = CreateRequest(HttpMethod.Delete, $"{Url}/{id}");
            WithActorHeaders(request, SeedInvitees[0]);

            var response = await Client.SendAsync(request, TestContext.Current.CancellationToken);
            var problem = await ReadProblemDetailsAsync(response, TestContext.Current.CancellationToken);

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
            Assert.NotNull(problem);
            Assert.Equal(nameof(Invitation), problem.Extensions["resource"]?.ToString());
            Assert.Equal(id, problem.Extensions["identifier"]?.ToString());
        }

        [Fact]
        public async Task ShouldReturnForbidden_WhenActorIsNotTheInvitedUser()
        {
            var existingInvitation = SeedInvitations.First(i => i.Status == InvitationStatusEnum.Open);
            var bystander = SeedInvitees.First(u => u.Id != existingInvitation.UserId);

            var request = CreateRequest(HttpMethod.Delete, $"{Url}/{existingInvitation.Id}");
            WithActorHeaders(request, bystander);

            var response = await Client.SendAsync(request, TestContext.Current.CancellationToken);
            var problem = await ReadProblemDetailsAsync(response, TestContext.Current.CancellationToken);

            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
            Assert.NotNull(problem);
            Assert.Equal("Action only available to subject user.", problem.Detail);
        }

        [Fact]
        public async Task ShouldReturnUnprocessableEntity_WhenInvitationIsAlreadyAccepted()
        {
            var existingInvitation = SeedInvitations.First(i => i.Status == InvitationStatusEnum.Accepted);
            var invitee = SeedInvitees.Single(u => u.Id == existingInvitation.UserId);

            var request = CreateRequest(HttpMethod.Delete, $"{Url}/{existingInvitation.Id}");
            WithActorHeaders(request, invitee);

            var response = await Client.SendAsync(request, TestContext.Current.CancellationToken);

            Assert.Equal(HttpStatusCode.UnprocessableEntity, response.StatusCode);
        }

        [Fact]
        public async Task ShouldReturnUnprocessableEntity_WhenInvitationIsAlreadyFailed()
        {
            var existingInvitation = SeedInvitations.First(i => i.Status == InvitationStatusEnum.Failed);
            var invitee = SeedInvitees.Single(u => u.Id == existingInvitation.UserId);

            var request = CreateRequest(HttpMethod.Delete, $"{Url}/{existingInvitation.Id}");
            WithActorHeaders(request, invitee);

            var response = await Client.SendAsync(request, TestContext.Current.CancellationToken);

            Assert.Equal(HttpStatusCode.UnprocessableEntity, response.StatusCode);
        }

        [Fact]
        public async Task ShouldReturnNoContent_WithUnchangedStatus_WhenAlreadyDeclined()
        {
            var existingInvitation = SeedInvitations.First(i => i.Status == InvitationStatusEnum.Declined);
            var invitee = SeedInvitees.Single(u => u.Id == existingInvitation.UserId);

            var request = CreateRequest(HttpMethod.Delete, $"{Url}/{existingInvitation.Id}");
            WithActorHeaders(request, invitee);

            var response = await Client.SendAsync(request, TestContext.Current.CancellationToken);

            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

            var getRequest = CreateRequest(HttpMethod.Get, $"{Url}/{existingInvitation.Id}");
            WithActorHeaders(getRequest, invitee);
            var getResponse = await Client.SendAsync(getRequest, TestContext.Current.CancellationToken);
            var detail = await ReadContentAsync<InvitationDetailModel>(getResponse, TestContext.Current.CancellationToken);

            Assert.NotNull(detail);
            Assert.Equal(nameof(InvitationStatusEnum.Declined), detail.Status);
        }

        [Fact]
        public async Task ShouldMarkInvitationAsFailed_WhenInviteeAlreadyHasAPlayerInGame()
        {
            var (_, invitee, invitation) = await SeedOpenInvitationAsync(inviteeAlreadyInGame: true);

            var request = CreateRequest(HttpMethod.Delete, $"{Url}/{invitation.Id}");
            WithActorHeaders(request, invitee);

            var response = await Client.SendAsync(request, TestContext.Current.CancellationToken);

            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

            var getRequest = CreateRequest(HttpMethod.Get, $"{Url}/{invitation.Id}");
            WithActorHeaders(getRequest, invitee);
            var getResponse = await Client.SendAsync(getRequest, TestContext.Current.CancellationToken);
            var detail = await ReadContentAsync<InvitationDetailModel>(getResponse, TestContext.Current.CancellationToken);

            Assert.NotNull(detail);
            Assert.Equal(nameof(InvitationStatusEnum.Failed), detail.Status);
        }

        [Fact]
        public async Task ShouldReturnNoContent_WhenRequestIsValid()
        {
            var (_, invitee, invitation) = await SeedOpenInvitationAsync();

            var request = CreateRequest(HttpMethod.Delete, $"{Url}/{invitation.Id}");
            WithActorHeaders(request, invitee);

            var response = await Client.SendAsync(request, TestContext.Current.CancellationToken);

            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

            var getRequest = CreateRequest(HttpMethod.Get, $"{Url}/{invitation.Id}");
            WithActorHeaders(getRequest, invitee);
            var getResponse = await Client.SendAsync(getRequest, TestContext.Current.CancellationToken);
            var detail = await ReadContentAsync<InvitationDetailModel>(getResponse, TestContext.Current.CancellationToken);

            Assert.NotNull(detail);
            Assert.Equal(nameof(InvitationStatusEnum.Declined), detail.Status);
        }
    }
}