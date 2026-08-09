using System.Net;
using Teams.Api.Controllers.V1.Users.ResponseModels;
using Teams.Domain.Entities;

namespace Teams.Api.IntegrationTests.Controllers.V1.Users;

public static partial class UsersControllerTests
{
    public class GetUserById(ApiWebApplicationFactory factory) : UsersControllerTestsBase(factory)
    {
        [Fact]
        public async Task ShouldReturnBadRequest_WhenVersionIsUnsupported()
        {
            var existingUser = SeedUsers[0];

            var request = CreateRequest(HttpMethod.Get, $"{VersionlessUrl}/{existingUser.Id}", apiVersion: "2.0");

            var response = await Client.SendAsync(request, TestContext.Current.CancellationToken);

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task ShouldReturnBadRequest_WhenVersionIsNotProvided()
        {
            var existingUser = SeedUsers[0];

            var request = CreateRequest(HttpMethod.Get, $"{VersionlessUrl}/{existingUser.Id}", apiVersion: null);

            var response = await Client.SendAsync(request, TestContext.Current.CancellationToken);

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task ShouldReturnNotFound_WhenUserDoesNotExist()
        {
            const string id = "does-not-exist";

            var request = CreateRequest(HttpMethod.Get, $"{Url}/{id}");

            var response = await Client.SendAsync(request, TestContext.Current.CancellationToken);
            var problem = await ReadProblemDetailsAsync(response, TestContext.Current.CancellationToken);

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
            Assert.NotNull(problem);
            Assert.Equal(nameof(User), problem.Extensions["resource"]?.ToString());
            Assert.Equal(id, problem.Extensions["identifier"]?.ToString());
        }

        [Fact]
        public async Task ShouldReturnNotFound_WhenUserIsDeleted()
        {
            var existingUser = SeedUsers[0];

            var deleteRequest = CreateRequest(HttpMethod.Delete, $"{Url}/{existingUser.Id}");
            WithActorHeaders(deleteRequest, existingUser);
            await Client.SendAsync(deleteRequest, TestContext.Current.CancellationToken);

            var request = CreateRequest(HttpMethod.Get, $"{Url}/{existingUser.Id}");

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

            var request = CreateRequest(HttpMethod.Get, $"{Url}/{existingUser.Id}");

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
        }
    }
}