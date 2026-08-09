using System.Net;
using Teams.Domain.Entities;

namespace Teams.Api.IntegrationTests.Controllers.V1.Users;

public static partial class UsersControllerTests
{
    public class DeleteUser(ApiWebApplicationFactory factory) : UsersControllerTestsBase(factory)
    {
        [Fact]
        public async Task ShouldReturnBadRequest_WhenVersionIsUnsupported()
        {
            var existingUser = SeedUsers[0];

            var request = CreateRequest(HttpMethod.Delete, $"{VersionlessUrl}/{existingUser.Id}", apiVersion: "2.0");
            WithActorHeaders(request, existingUser);

            var response = await Client.SendAsync(request, TestContext.Current.CancellationToken);

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task ShouldReturnBadRequest_WhenVersionIsNotProvided()
        {
            var existingUser = SeedUsers[0];

            var request = CreateRequest(HttpMethod.Delete, $"{VersionlessUrl}/{existingUser.Id}", apiVersion: null);
            WithActorHeaders(request, existingUser);

            var response = await Client.SendAsync(request, TestContext.Current.CancellationToken);

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task ShouldReturnPreconditionRequired_WhenActorHeadersAreMissing()
        {
            var existingUser = SeedUsers[0];

            var request = CreateRequest(HttpMethod.Delete, $"{Url}/{existingUser.Id}");

            var response = await Client.SendAsync(request, TestContext.Current.CancellationToken);
            var problem = await ReadProblemDetailsAsync(response, TestContext.Current.CancellationToken);

            Assert.Equal(HttpStatusCode.PreconditionRequired, response.StatusCode);
            Assert.NotNull(problem);
            Assert.Equal("'Teams-User-Id' header value is required.", problem.Detail);
        }

        [Fact]
        public async Task ShouldReturnForbidden_WhenActorIsNotSelf()
        {
            var existingUser = SeedUsers[0];
            var otherUser = SeedUsers[1];

            var request = CreateRequest(HttpMethod.Delete, $"{Url}/{existingUser.Id}");
            WithActorHeaders(request, otherUser);

            var response = await Client.SendAsync(request, TestContext.Current.CancellationToken);
            var problem = await ReadProblemDetailsAsync(response, TestContext.Current.CancellationToken);

            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
            Assert.NotNull(problem);
            Assert.Equal("Action only available to subject user.", problem.Detail);
        }

        [Fact]
        public async Task ShouldReturnNotFound_WhenUserDoesNotExist()
        {
            const string id = "does-not-exist";

            var request = CreateRequest(HttpMethod.Delete, $"{Url}/{id}");
            WithActorHeaders(request, id, "does-not-exist-tag", "Ghost User");

            var response = await Client.SendAsync(request, TestContext.Current.CancellationToken);
            var problem = await ReadProblemDetailsAsync(response, TestContext.Current.CancellationToken);

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
            Assert.NotNull(problem);
            Assert.Equal(nameof(User), problem.Extensions["resource"]?.ToString());
            Assert.Equal(id, problem.Extensions["identifier"]?.ToString());
        }

        [Fact]
        public async Task ShouldReturnNotFound_WhenUserIsAlreadyDeleted()
        {
            var existingUser = SeedUsers[0];

            var deleteRequest = CreateRequest(HttpMethod.Delete, $"{Url}/{existingUser.Id}");
            WithActorHeaders(deleteRequest, existingUser);
            await Client.SendAsync(deleteRequest, TestContext.Current.CancellationToken);

            var secondDeleteRequest = CreateRequest(HttpMethod.Delete, $"{Url}/{existingUser.Id}");
            WithActorHeaders(secondDeleteRequest, existingUser);

            var response = await Client.SendAsync(secondDeleteRequest, TestContext.Current.CancellationToken);
            var problem = await ReadProblemDetailsAsync(response, TestContext.Current.CancellationToken);

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
            Assert.NotNull(problem);
            Assert.Equal(existingUser.Id, problem.Extensions["identifier"]?.ToString());
        }

        [Fact]
        public async Task ShouldReturnNoContent_WithUserNoLongerRetrievable_WhenRequestIsValid()
        {
            var existingUser = SeedUsers[0];

            var request = CreateRequest(HttpMethod.Delete, $"{Url}/{existingUser.Id}");
            WithActorHeaders(request, existingUser);

            var response = await Client.SendAsync(request, TestContext.Current.CancellationToken);

            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

            var getRequest = CreateRequest(HttpMethod.Get, $"{Url}/{existingUser.Id}");
            var getResponse = await Client.SendAsync(getRequest, TestContext.Current.CancellationToken);

            Assert.Equal(HttpStatusCode.NotFound, getResponse.StatusCode);
        }
    }
}