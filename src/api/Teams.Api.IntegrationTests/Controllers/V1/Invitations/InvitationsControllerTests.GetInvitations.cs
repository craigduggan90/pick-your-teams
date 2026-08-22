using System.Net;
using Teams.Api.Controllers.V1.Invitations.ResponseModels;
using Teams.Common.Pagination;
using Teams.Domain.Enums;

namespace Teams.Api.IntegrationTests.Controllers.V1.Invitations;

public static partial class InvitationsControllerTests
{
    public class GetInvitations(ApiWebApplicationFactory factory) : InvitationsControllerTestsBase(factory)
    {
        [Fact]
        public async Task ShouldReturnBadRequest_WhenVersionIsUnsupported()
        {
            var request = CreateRequest(HttpMethod.Get, WithQuery(VersionlessUrl), apiVersion: "2.0");
            WithActorHeaders(request, Organiser);

            var response = await Client.SendAsync(request, TestContext.Current.CancellationToken);

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task ShouldReturnBadRequest_WhenVersionIsNotProvided()
        {
            var request = CreateRequest(HttpMethod.Get, WithQuery(VersionlessUrl), apiVersion: null);
            WithActorHeaders(request, Organiser);

            var response = await Client.SendAsync(request, TestContext.Current.CancellationToken);

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task ShouldReturnOk_WithDefaultPageSize_WhenNoFiltersProvided()
        {
            var request = CreateRequest(HttpMethod.Get, Url);
            WithActorHeaders(request, Organiser);

            var response = await Client.SendAsync(request, TestContext.Current.CancellationToken);
            var content = await ReadContentAsync<PagedList<InvitationModel>>(response, TestContext.Current.CancellationToken);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.NotNull(content);
            Assert.Equal(25, content.Data.Count); // 30 seed invitations, default page size 25
        }

        [Fact]
        public async Task ShouldReturnOk_WithPagedList_FilteredByEmailAddress_WhenEmailAddressProvided()
        {
            var invitee = SeedInvitees[2];

            var url = WithQuery(Url, ("EmailAddress", invitee.EmailAddress), ("PageSize", "100"));
            var request = CreateRequest(HttpMethod.Get, url);
            WithActorHeaders(request, Organiser);

            var response = await Client.SendAsync(request, TestContext.Current.CancellationToken);
            var content = await ReadContentAsync<PagedList<InvitationModel>>(response, TestContext.Current.CancellationToken);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.NotNull(content);
            Assert.Equal(6, content.Data.Count); // 30 invitations / 5 invitees = 6 each
        }

        [Fact]
        public async Task ShouldReturnOk_WithPagedList_FilteredByStatus_WhenStatusProvided()
        {
            var url = WithQuery(Url, ("Status", nameof(InvitationStatusEnum.Accepted)), ("PageSize", "100"));
            var request = CreateRequest(HttpMethod.Get, url);
            WithActorHeaders(request, Organiser);

            var response = await Client.SendAsync(request, TestContext.Current.CancellationToken);
            var content = await ReadContentAsync<PagedList<InvitationModel>>(response, TestContext.Current.CancellationToken);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.NotNull(content);
            Assert.Equal(8, content.Data.Count); // index % 4 == 1 across 1..30
            Assert.All(content.Data, i => Assert.Equal(nameof(InvitationStatusEnum.Accepted), i.Status));
        }

        [Fact]
        public async Task ShouldReturnOk_WithPagedList_FilteredByCreatedFrom_WhenCreatedFromProvided()
        {
            var cutoff = SeedInvitations[14].DateCreated; // the 15th seeded invitation

            var url = WithQuery(Url, ("CreatedFrom", cutoff.ToString("O")), ("PageSize", "100"));
            var request = CreateRequest(HttpMethod.Get, url);
            WithActorHeaders(request, Organiser);

            var response = await Client.SendAsync(request, TestContext.Current.CancellationToken);
            var content = await ReadContentAsync<PagedList<InvitationModel>>(response, TestContext.Current.CancellationToken);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.NotNull(content);
            Assert.Equal(16, content.Data.Count); // inclusive: invitations 15 through 30
        }

        [Fact]
        public async Task ShouldReturnOk_WithPagedList_FilteredByCreatedTo_WhenCreatedToProvided()
        {
            var cutoff = SeedInvitations[14].DateCreated;

            var url = WithQuery(Url, ("CreatedTo", cutoff.ToString("O")), ("PageSize", "100"));
            var request = CreateRequest(HttpMethod.Get, url);
            WithActorHeaders(request, Organiser);

            var response = await Client.SendAsync(request, TestContext.Current.CancellationToken);
            var content = await ReadContentAsync<PagedList<InvitationModel>>(response, TestContext.Current.CancellationToken);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.NotNull(content);
            Assert.Equal(14, content.Data.Count); // exclusive: invitations 1 through 14
        }

        [Fact]
        public async Task ShouldReturnForbidden_WhenGameIdProvidedAndActorIsNotOrganiser()
        {
            var bystander = SeedInvitees[0];

            var url = WithQuery(Url, ("GameId", SeedGame.Id));
            var request = CreateRequest(HttpMethod.Get, url);
            WithActorHeaders(request, bystander);

            var response = await Client.SendAsync(request, TestContext.Current.CancellationToken);

            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        }

        [Fact]
        public async Task ShouldReturnOk_WithPagedList_FilteredByGameId_WhenActorIsOrganiser()
        {
            var url = WithQuery(Url, ("GameId", SeedGame.Id), ("PageSize", "100"));
            var request = CreateRequest(HttpMethod.Get, url);
            WithActorHeaders(request, Organiser);

            var response = await Client.SendAsync(request, TestContext.Current.CancellationToken);
            var content = await ReadContentAsync<PagedList<InvitationModel>>(response, TestContext.Current.CancellationToken);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.NotNull(content);
            Assert.Equal(30, content.Data.Count);
            Assert.All(content.Data, item => Assert.NotNull(item.Invitee));
            Assert.All(content.Data, item => Assert.NotEqual(default, item.Created));
        }

        [Fact]
        public async Task ShouldReturnForbidden_WhenUserIdProvidedAndActorIsNotThatUser()
        {
            var invitee = SeedInvitees[0];
            var bystander = SeedInvitees[1];

            var url = WithQuery(Url, ("UserId", invitee.Id));
            var request = CreateRequest(HttpMethod.Get, url);
            WithActorHeaders(request, bystander);

            var response = await Client.SendAsync(request, TestContext.Current.CancellationToken);

            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        }

        [Fact]
        public async Task ShouldReturnOk_WithPagedList_FilteredByUserId_WhenActorIsThatUser()
        {
            var invitee = SeedInvitees[0];

            var url = WithQuery(Url, ("UserId", invitee.Id), ("PageSize", "100"));
            var request = CreateRequest(HttpMethod.Get, url);
            WithActorHeaders(request, invitee);

            var response = await Client.SendAsync(request, TestContext.Current.CancellationToken);
            var content = await ReadContentAsync<PagedList<InvitationModel>>(response, TestContext.Current.CancellationToken);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.NotNull(content);
            Assert.Equal(6, content.Data.Count); // 30 invitations / 5 invitees = 6 each
        }

        [Fact]
        public async Task ShouldReturnOk_WithPagedList_FilteredByCursor_WhenCursorProvided()
        {
            var firstPageUrl = WithQuery(Url, ("PageSize", "10"));
            var firstPageRequest = CreateRequest(HttpMethod.Get, firstPageUrl);
            WithActorHeaders(firstPageRequest, Organiser);
            var firstPageResponse = await Client.SendAsync(firstPageRequest, TestContext.Current.CancellationToken);
            var firstPage = await ReadContentAsync<PagedList<InvitationModel>>(firstPageResponse, TestContext.Current.CancellationToken);

            Assert.NotNull(firstPage);
            Assert.NotNull(firstPage.Cursor);

            var secondPageUrl = WithQuery(Url, ("PageSize", "10"), ("Cursor", firstPage.Cursor));
            var secondPageRequest = CreateRequest(HttpMethod.Get, secondPageUrl);
            WithActorHeaders(secondPageRequest, Organiser);
            var secondPageResponse = await Client.SendAsync(secondPageRequest, TestContext.Current.CancellationToken);
            var secondPage = await ReadContentAsync<PagedList<InvitationModel>>(secondPageResponse, TestContext.Current.CancellationToken);

            Assert.Equal(HttpStatusCode.OK, secondPageResponse.StatusCode);
            Assert.NotNull(secondPage);
            Assert.Equal(10, secondPage.Data.Count);
            Assert.Empty(firstPage.Data.Select(i => i.Id).Intersect(secondPage.Data.Select(i => i.Id)));
        }

        [Fact]
        public async Task ShouldReturnOk_WithPagedList_FilteredByPageSize_WhenPageSizeProvided()
        {
            var url = WithQuery(Url, ("PageSize", "5"));
            var request = CreateRequest(HttpMethod.Get, url);
            WithActorHeaders(request, Organiser);

            var response = await Client.SendAsync(request, TestContext.Current.CancellationToken);
            var content = await ReadContentAsync<PagedList<InvitationModel>>(response, TestContext.Current.CancellationToken);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.NotNull(content);
            Assert.Equal(5, content.Data.Count);
        }
    }
}