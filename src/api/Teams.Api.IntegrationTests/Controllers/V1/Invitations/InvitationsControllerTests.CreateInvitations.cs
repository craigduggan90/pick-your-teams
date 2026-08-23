using System.Net;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Teams.Api.Controllers.V1.Invitations.RequestModels;
using Teams.Api.Controllers.V1.Invitations.ResponseModels;
using Teams.Common.Pagination;
using Teams.Data.Context;
using Teams.Domain.Entities;

namespace Teams.Api.IntegrationTests.Controllers.V1.Invitations;

public static partial class InvitationsControllerTests
{
    public class CreateInvitations(ApiWebApplicationFactory factory) : InvitationsControllerTestsBase(factory)
    {
        [Fact]
        public async Task ShouldReturnBadRequest_WhenVersionIsUnsupported()
        {
            var request = CreateJsonRequest(
                HttpMethod.Post, VersionlessUrl, new CreateInvitationsRequestModel(SeedGame.Id, [SeedInvitees[0].Tag]), apiVersion: "2.0");
            WithActorHeaders(request, Organiser);

            var response = await Client.SendAsync(request, TestContext.Current.CancellationToken);

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task ShouldReturnBadRequest_WhenVersionIsNotProvided()
        {
            var request = CreateJsonRequest(
                HttpMethod.Post, VersionlessUrl, new CreateInvitationsRequestModel(SeedGame.Id, [SeedInvitees[0].Tag]), apiVersion: null);
            WithActorHeaders(request, Organiser);

            var response = await Client.SendAsync(request, TestContext.Current.CancellationToken);

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task ShouldReturnPreconditionRequired_WhenActorHeadersAreMissing()
        {
            var request = CreateJsonRequest(HttpMethod.Post, Url, new CreateInvitationsRequestModel(SeedGame.Id, [SeedInvitees[0].Tag]));

            var response = await Client.SendAsync(request, TestContext.Current.CancellationToken);
            var problem = await ReadProblemDetailsAsync(response, TestContext.Current.CancellationToken);

            Assert.Equal(HttpStatusCode.PreconditionRequired, response.StatusCode);
            Assert.NotNull(problem);
            Assert.Equal("'Teams-User-Id' header value is required.", problem.Detail);
        }

        [Fact]
        public async Task ShouldReturnForbidden_WhenActorIsNotOrganiser()
        {
            var nonOrganiser = SeedInvitees[0];

            var request = CreateJsonRequest(HttpMethod.Post, Url, new CreateInvitationsRequestModel(SeedGame.Id, [SeedInvitees[1].Tag]));
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

            var request = CreateJsonRequest(HttpMethod.Post, Url, new CreateInvitationsRequestModel(id, [SeedInvitees[0].Tag]));
            WithActorHeaders(request, Organiser);

            var response = await Client.SendAsync(request, TestContext.Current.CancellationToken);
            var problem = await ReadProblemDetailsAsync(response, TestContext.Current.CancellationToken);

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
            Assert.NotNull(problem);
            Assert.Equal(nameof(Game), problem.Extensions["resource"]?.ToString());
            Assert.Equal(id, problem.Extensions["identifier"]?.ToString());
        }

        [Fact]
        public async Task ShouldReturnUnprocessableEntity_WhenUserTagsIsEmpty()
        {
            var request = CreateJsonRequest(HttpMethod.Post, Url, new CreateInvitationsRequestModel(SeedGame.Id, []));
            WithActorHeaders(request, Organiser);

            var response = await Client.SendAsync(request, TestContext.Current.CancellationToken);

            Assert.Equal(HttpStatusCode.UnprocessableEntity, response.StatusCode);
        }

        [Fact]
        public async Task ShouldReturnUnprocessableEntity_WhenTagsAreDuplicated()
        {
            var tag = SeedInvitees[0].Tag;

            var request = CreateJsonRequest(HttpMethod.Post, Url, new CreateInvitationsRequestModel(SeedGame.Id, [tag, tag]));
            WithActorHeaders(request, Organiser);

            var response = await Client.SendAsync(request, TestContext.Current.CancellationToken);

            Assert.Equal(HttpStatusCode.UnprocessableEntity, response.StatusCode);
        }

        [Fact]
        public async Task ShouldReturnUnprocessableEntity_WhenATagIsNotValidFormat()
        {
            var request = CreateJsonRequest(HttpMethod.Post, Url, new CreateInvitationsRequestModel(SeedGame.Id, ["ab"]));
            WithActorHeaders(request, Organiser);

            var response = await Client.SendAsync(request, TestContext.Current.CancellationToken);

            Assert.Equal(HttpStatusCode.UnprocessableEntity, response.StatusCode);
        }

        [Fact]
        public async Task ShouldReturnUnprocessableEntity_WhenATagDoesNotMatchAUser()
        {
            const string tag = "does-not-match-anyone";

            var request = CreateJsonRequest(HttpMethod.Post, Url, new CreateInvitationsRequestModel(SeedGame.Id, [tag]));
            WithActorHeaders(request, Organiser);

            var response = await Client.SendAsync(request, TestContext.Current.CancellationToken);
            var problem = await ReadProblemDetailsAsync(response, TestContext.Current.CancellationToken);

            Assert.Equal(HttpStatusCode.UnprocessableEntity, response.StatusCode);
            Assert.NotNull(problem);
            Assert.Contains(tag, GetAllValidationErrors(problem).Single());
        }

        [Fact]
        public async Task ShouldReturnCreated_WithInvitationPersisted_WhenRequestIsValid()
        {
            var invitee = SeedInvitees[0];

            var request = CreateJsonRequest(HttpMethod.Post, Url, new CreateInvitationsRequestModel(SeedGame.Id, [invitee.Tag]));
            WithActorHeaders(request, Organiser);

            var response = await Client.SendAsync(request, TestContext.Current.CancellationToken);

            Assert.Equal(HttpStatusCode.Created, response.StatusCode);

            var getUrl = WithQuery(Url, ("UserId", invitee.Id), ("PageSize", "100"));
            var getRequest = CreateRequest(HttpMethod.Get, getUrl);
            WithActorHeaders(getRequest, invitee);
            var getResponse = await Client.SendAsync(getRequest, TestContext.Current.CancellationToken);
            var content = await ReadContentAsync<PagedList<InvitationModel>>(getResponse, TestContext.Current.CancellationToken);

            Assert.NotNull(content);
            Assert.Contains(content.Data, i => i.Game.Id == SeedGame.Id);
        }

        [Fact]
        public async Task ShouldReturnCreated_WithInvitationPersisted_WhenTagCasingDiffersFromStoredTag()
        {
            var invitee = EntityFactory.CreateUser(id: "case-test-invitee", displayName: "Case Test Invitee");
            await using (var scope = Factory.Services.CreateAsyncScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<ApiDbContext>();
                await context.Users.AddAsync(invitee, TestContext.Current.CancellationToken);
                await context.SaveChangesAsync(TestContext.Current.CancellationToken);
            }

            var request = CreateJsonRequest(
                HttpMethod.Post, Url, new CreateInvitationsRequestModel(SeedGame.Id, [invitee.Tag.ToUpperInvariant()]));
            WithActorHeaders(request, Organiser);

            var response = await Client.SendAsync(request, TestContext.Current.CancellationToken);

            Assert.Equal(HttpStatusCode.Created, response.StatusCode);

            var getUrl = WithQuery(Url, ("UserId", invitee.Id), ("PageSize", "100"));
            var getRequest = CreateRequest(HttpMethod.Get, getUrl);
            WithActorHeaders(getRequest, invitee);
            var getResponse = await Client.SendAsync(getRequest, TestContext.Current.CancellationToken);
            var content = await ReadContentAsync<PagedList<InvitationModel>>(getResponse, TestContext.Current.CancellationToken);

            Assert.NotNull(content);
            Assert.Single(content.Data);
        }

        [Fact]
        public async Task ShouldReturnCreated_ButNotDuplicateTheInvitation_WhenInviteeAlreadyHasAnOpenInvitationToTheGame()
        {
            var invitee = EntityFactory.CreateUser(id: "duplicate-test-invitee", displayName: "Duplicate Test Invitee");
            var existingInvitation = EntityFactory.CreateInvitation(SeedGame.Id, invitee.Id, invitee.EmailAddress);
            await using (var scope = Factory.Services.CreateAsyncScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<ApiDbContext>();
                await context.Users.AddAsync(invitee, TestContext.Current.CancellationToken);
                await context.Invitations.AddAsync(existingInvitation, TestContext.Current.CancellationToken);
                await context.SaveChangesAsync(TestContext.Current.CancellationToken);
            }

            var request = CreateJsonRequest(HttpMethod.Post, Url, new CreateInvitationsRequestModel(SeedGame.Id, [invitee.Tag]));
            WithActorHeaders(request, Organiser);

            var response = await Client.SendAsync(request, TestContext.Current.CancellationToken);

            Assert.Equal(HttpStatusCode.Created, response.StatusCode);

            var getUrl = WithQuery(Url, ("UserId", invitee.Id), ("PageSize", "100"));
            var getRequest = CreateRequest(HttpMethod.Get, getUrl);
            WithActorHeaders(getRequest, invitee);
            var getResponse = await Client.SendAsync(getRequest, TestContext.Current.CancellationToken);
            var content = await ReadContentAsync<PagedList<InvitationModel>>(getResponse, TestContext.Current.CancellationToken);

            Assert.NotNull(content);
            Assert.Single(content.Data);
            Assert.Equal(existingInvitation.Id, content.Data.Single().Id);
        }
    }
}