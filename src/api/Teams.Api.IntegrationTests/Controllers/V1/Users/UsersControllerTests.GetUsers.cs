using System.Net;
using Teams.Api.Controllers.V1.Users.ResponseModels;
using Teams.Common.Pagination;

namespace Teams.Api.IntegrationTests.Controllers.V1.Users;

public static partial class UsersControllerTests
{
    public class GetUsers(ApiWebApplicationFactory factory) : UsersControllerTestsBase(factory)
    {
        [Fact]
        public async Task ShouldReturnBadRequest_WhenVersionIsUnsupported()
        {
            var url = WithQuery(VersionlessUrl);
            var request = CreateRequest(HttpMethod.Get, url, apiVersion: "2.0");

            var response = await Client.SendAsync(request, TestContext.Current.CancellationToken);

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task ShouldReturnBadRequest_WhenVersionIsNotProvided()
        {
            var url = WithQuery(VersionlessUrl);
            var request = CreateRequest(HttpMethod.Get, url, apiVersion: null);

            var response = await Client.SendAsync(request, TestContext.Current.CancellationToken);

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task ShouldReturnOk_WithDefaultPageSize_WhenNoFiltersProvided()
        {
            var request = CreateRequest(HttpMethod.Get, Url);

            var response = await Client.SendAsync(request, TestContext.Current.CancellationToken);
            var content = await ReadContentAsync<PagedList<UserModel>>(response, TestContext.Current.CancellationToken);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.NotNull(content);
            Assert.Equal(25, content.Data.Count); // 30 seed users, default page size 25
            Assert.Equal(25, content.Count);
        }

        [Fact]
        public async Task ShouldReturnOk_WithPagedList_FilteredByEmailAddress_WhenEmailAddressProvided()
        {
            var existingUser = SeedUsers[14];

            var url = WithQuery(Url, ("EmailAddress", existingUser.EmailAddress));
            var request = CreateRequest(HttpMethod.Get, url);

            var response = await Client.SendAsync(request, TestContext.Current.CancellationToken);
            var content = await ReadContentAsync<PagedList<UserModel>>(response, TestContext.Current.CancellationToken);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.NotNull(content);
            Assert.Equal([existingUser.Id], content.Data.Select(u => u.Id));
        }

        [Fact]
        public async Task ShouldReturnOk_WithPagedList_FilteredByTag_WhenTagProvided()
        {
            var existingUser = SeedUsers[14];

            var url = WithQuery(Url, ("Tag", existingUser.Tag));
            var request = CreateRequest(HttpMethod.Get, url);

            var response = await Client.SendAsync(request, TestContext.Current.CancellationToken);
            var content = await ReadContentAsync<PagedList<UserModel>>(response, TestContext.Current.CancellationToken);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.NotNull(content);
            Assert.Equal([existingUser.Id], content.Data.Select(u => u.Id));
        }

        [Fact]
        public async Task ShouldReturnOk_WithPagedList_FilteredByDisplayName_WhenDisplayNameProvided()
        {
            var existingUser = SeedUsers[14];

            var url = WithQuery(Url, ("DisplayName", existingUser.DisplayName));
            var request = CreateRequest(HttpMethod.Get, url);

            var response = await Client.SendAsync(request, TestContext.Current.CancellationToken);
            var content = await ReadContentAsync<PagedList<UserModel>>(response, TestContext.Current.CancellationToken);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.NotNull(content);
            Assert.Equal([existingUser.Id], content.Data.Select(u => u.Id));
        }

        [Fact]
        public async Task ShouldReturnOk_WithPagedList_FilteredByRatingFrom_WhenRatingFromProvided()
        {
            var url = WithQuery(Url, ("RatingFrom", "1015"), ("PageSize", "100"));
            var request = CreateRequest(HttpMethod.Get, url);

            var response = await Client.SendAsync(request, TestContext.Current.CancellationToken);
            var content = await ReadContentAsync<PagedList<UserModel>>(response, TestContext.Current.CancellationToken);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.NotNull(content);
            Assert.Equal(16, content.Data.Count); // inclusive: ratings 1015 - 1030 (users 15 - 30)
        }

        [Fact]
        public async Task ShouldReturnOk_WithPagedList_FilteredByRatingTo_WhenRatingToProvided()
        {
            var url = WithQuery(Url, ("RatingTo", "1015"), ("PageSize", "100"));
            var request = CreateRequest(HttpMethod.Get, url);

            var response = await Client.SendAsync(request, TestContext.Current.CancellationToken);
            var content = await ReadContentAsync<PagedList<UserModel>>(response, TestContext.Current.CancellationToken);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.NotNull(content);
            Assert.Equal(14, content.Data.Count); // exclusive: ratings 1001 - 1014 (users 1 - 14)
        }

        [Fact]
        public async Task ShouldReturnOk_WithPagedList_FilteredByCreatedFrom_WhenCreatedFromProvided()
        {
            var cutoff = SeedUsers[14].DateCreated; // the 15th seeded user

            var url = WithQuery(Url, ("CreatedFrom", cutoff.ToString("O")), ("PageSize", "100"));
            var request = CreateRequest(HttpMethod.Get, url);

            var response = await Client.SendAsync(request, TestContext.Current.CancellationToken);
            var content = await ReadContentAsync<PagedList<UserModel>>(response, TestContext.Current.CancellationToken);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.NotNull(content);
            Assert.Equal(16, content.Data.Count); // inclusive: users 15 through 30
        }

        [Fact]
        public async Task ShouldReturnOk_WithPagedList_FilteredByCreatedTo_WhenCreatedToProvided()
        {
            var cutoff = SeedUsers[14].DateCreated; // the 15th seeded user

            var url = WithQuery(Url, ("CreatedTo", cutoff.ToString("O")), ("PageSize", "100"));
            var request = CreateRequest(HttpMethod.Get, url);

            var response = await Client.SendAsync(request, TestContext.Current.CancellationToken);
            var content = await ReadContentAsync<PagedList<UserModel>>(response, TestContext.Current.CancellationToken);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.NotNull(content);
            Assert.Equal(14, content.Data.Count); // exclusive: users 1 through 14
        }

        [Fact]
        public async Task ShouldReturnOk_WithPagedList_FilteredByModifiedFrom_WhenModifiedFromProvided()
        {
            // Seed data sets DateModified equal to DateCreated for every user, so this mirrors the CreatedFrom test.
            var cutoff = SeedUsers[14].DateCreated;

            var url = WithQuery(Url, ("ModifiedFrom", cutoff.ToString("O")), ("PageSize", "100"));
            var request = CreateRequest(HttpMethod.Get, url);

            var response = await Client.SendAsync(request, TestContext.Current.CancellationToken);
            var content = await ReadContentAsync<PagedList<UserModel>>(response, TestContext.Current.CancellationToken);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.NotNull(content);
            Assert.Equal(16, content.Data.Count);
        }

        [Fact]
        public async Task ShouldReturnOk_WithPagedList_FilteredByModifiedTo_WhenModifiedToProvided()
        {
            var cutoff = SeedUsers[14].DateCreated;

            var url = WithQuery(Url, ("ModifiedTo", cutoff.ToString("O")), ("PageSize", "100"));
            var request = CreateRequest(HttpMethod.Get, url);

            var response = await Client.SendAsync(request, TestContext.Current.CancellationToken);
            var content = await ReadContentAsync<PagedList<UserModel>>(response, TestContext.Current.CancellationToken);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.NotNull(content);
            Assert.Equal(14, content.Data.Count);
        }

        [Fact]
        public async Task ShouldReturnOk_WithPagedList_FilteredByCursor_WhenCursorProvided()
        {
            var firstPageUrl = WithQuery(Url, ("PageSize", "10"));
            var firstPageRequest = CreateRequest(HttpMethod.Get, firstPageUrl);
            var firstPageResponse = await Client.SendAsync(firstPageRequest, TestContext.Current.CancellationToken);
            var firstPage = await ReadContentAsync<PagedList<UserModel>>(firstPageResponse, TestContext.Current.CancellationToken);

            Assert.NotNull(firstPage);
            Assert.NotNull(firstPage.Cursor);

            var secondPageUrl = WithQuery(Url, ("PageSize", "10"), ("Cursor", firstPage.Cursor));
            var secondPageRequest = CreateRequest(HttpMethod.Get, secondPageUrl);
            var secondPageResponse = await Client.SendAsync(secondPageRequest, TestContext.Current.CancellationToken);
            var secondPage = await ReadContentAsync<PagedList<UserModel>>(secondPageResponse, TestContext.Current.CancellationToken);

            Assert.Equal(HttpStatusCode.OK, secondPageResponse.StatusCode);
            Assert.NotNull(secondPage);
            Assert.Equal(10, secondPage.Data.Count);
            Assert.Empty(firstPage.Data.Select(u => u.Id).Intersect(secondPage.Data.Select(u => u.Id)));
        }

        [Fact]
        public async Task ShouldReturnOk_WithPagedList_FilteredByPageSize_WhenPageSizeProvided()
        {
            var url = WithQuery(Url, ("PageSize", "5"));
            var request = CreateRequest(HttpMethod.Get, url);

            var response = await Client.SendAsync(request, TestContext.Current.CancellationToken);
            var content = await ReadContentAsync<PagedList<UserModel>>(response, TestContext.Current.CancellationToken);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.NotNull(content);
            Assert.Equal(5, content.Data.Count);
            Assert.Equal(5, content.Count);
        }
    }
}