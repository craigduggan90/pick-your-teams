using System.Net;
using Teams.Api.Controllers.V1.Users.ResponseModels;
using Teams.Api.Infrastructure;

namespace Teams.Api.IntegrationTests.Controllers.V1.Users;

public static partial class UsersControllerTests
{
    public class GetUserByExternalId(ApiWebApplicationFactory factory) : UsersControllerTestsBase(factory)
    {
        [Fact]
        public async Task ShouldReturnForbidden_WhenScopeIsMissing()
        {
            var existingUser = SeedUsers[0];

            var request = CreateRequest(HttpMethod.Get, $"{Url}/external/{existingUser.ExternalId}");

            var response = await Client.SendAsync(request, TestContext.Current.CancellationToken);

            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        }

        [Fact]
        public async Task ShouldReturnForbidden_WhenScopeDoesNotIncludeAuthoriser()
        {
            var existingUser = SeedUsers[0];

            var request = CreateRequest(HttpMethod.Get, $"{Url}/external/{existingUser.ExternalId}", scopes: "jobs:read");

            var response = await Client.SendAsync(request, TestContext.Current.CancellationToken);

            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        }

        [Fact]
        public async Task ShouldReturnNotFound_WhenUserDoesNotExist()
        {
            const string externalId = "does-not-exist";

            var request = CreateRequest(HttpMethod.Get, $"{Url}/external/{externalId}", scopes: Scopes.Authoriser);

            var response = await Client.SendAsync(request, TestContext.Current.CancellationToken);
            var problem = await ReadProblemDetailsAsync(response, TestContext.Current.CancellationToken);

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
            Assert.NotNull(problem);
            Assert.Equal(externalId, problem.Extensions["identifier"]?.ToString());
        }

        [Fact]
        public async Task ShouldReturnOk_WithUserContent_WhenUserExists()
        {
            var existingUser = SeedUsers[0];

            var request = CreateRequest(
                HttpMethod.Get, $"{Url}/external/{existingUser.ExternalId}", scopes: Scopes.Authoriser);

            var response = await Client.SendAsync(request, TestContext.Current.CancellationToken);
            var content = await ReadContentAsync<UserModel>(response, TestContext.Current.CancellationToken);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.NotNull(content);
            Assert.Equal(existingUser.Id, content.Id);
            Assert.Equal(existingUser.Tag, content.Tag);
            Assert.Equal(existingUser.DisplayName, content.DisplayName);
            Assert.Equal(existingUser.Rating, content.Rating);
        }
    }
}