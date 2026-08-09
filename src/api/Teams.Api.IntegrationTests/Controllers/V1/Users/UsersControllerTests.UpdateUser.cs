using System.Net;
using Teams.Api.Controllers.V1.Users.RequestModels;
using Teams.Api.Controllers.V1.Users.ResponseModels;
using Teams.Domain.Entities;

namespace Teams.Api.IntegrationTests.Controllers.V1.Users;

public static partial class UsersControllerTests
{
    public class UpdateUser(ApiWebApplicationFactory factory) : UsersControllerTestsBase(factory)
    {
        private static UpdateUserRequestModel ValidRequest =>
            new(Tag: null, DisplayName: "Jane Smith", Email: null, Mobile: null);

        [Fact]
        public async Task ShouldReturnBadRequest_WhenVersionIsUnsupported()
        {
            var existingUser = SeedUsers[0];

            var request = CreateJsonRequest(
                HttpMethod.Patch,
                $"{VersionlessUrl}/{existingUser.Id}",
                ValidRequest,
                apiVersion: "2.0");
            WithActorHeaders(request, existingUser);

            var response = await Client.SendAsync(request, TestContext.Current.CancellationToken);

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task ShouldReturnBadRequest_WhenVersionIsNotProvided()
        {
            var existingUser = SeedUsers[0];

            var request = CreateJsonRequest(
                HttpMethod.Patch,
                $"{VersionlessUrl}/{existingUser.Id}",
                ValidRequest,
                apiVersion: null);
            WithActorHeaders(request, existingUser);

            var response = await Client.SendAsync(request, TestContext.Current.CancellationToken);

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task ShouldReturnPreconditionRequired_WhenActorHeadersAreMissing()
        {
            var existingUser = SeedUsers[0];

            var request = CreateJsonRequest(HttpMethod.Patch, $"{Url}/{existingUser.Id}", ValidRequest);

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

            var request = CreateJsonRequest(HttpMethod.Patch, $"{Url}/{existingUser.Id}", ValidRequest);
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

            var request = CreateJsonRequest(HttpMethod.Patch, $"{Url}/{id}", ValidRequest);
            WithActorHeaders(request, id, "does-not-exist-tag", "Ghost User");

            var response = await Client.SendAsync(request, TestContext.Current.CancellationToken);
            var problem = await ReadProblemDetailsAsync(response, TestContext.Current.CancellationToken);

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
            Assert.NotNull(problem);
            Assert.Equal(nameof(User), problem.Extensions["resource"]?.ToString());
            Assert.Equal(id, problem.Extensions["identifier"]?.ToString());
        }

        [Fact]
        public async Task ShouldReturnUnprocessableEntity_WhenTagIsInvalid()
        {
            var existingUser = SeedUsers[0];
            var invalidRequest = ValidRequest with { Tag = "a" }; // below the 3 character minimum

            var request = CreateJsonRequest(HttpMethod.Patch, $"{Url}/{existingUser.Id}", invalidRequest);
            WithActorHeaders(request, existingUser);

            var response = await Client.SendAsync(request, TestContext.Current.CancellationToken);
            var problem = await ReadProblemDetailsAsync(response, TestContext.Current.CancellationToken);

            Assert.Equal(HttpStatusCode.UnprocessableEntity, response.StatusCode);
            Assert.NotNull(problem);
            Assert.Contains(GetValidationErrors(problem, nameof(UpdateUserRequestModel.Tag)),
                error => error.Contains("characters", StringComparison.OrdinalIgnoreCase));
        }

        [Fact]
        public async Task ShouldReturnUnprocessableEntity_WhenTagIsAlreadyInUseByAnotherUser()
        {
            var existingUser = SeedUsers[0];
            var otherUser = SeedUsers[1];
            var invalidRequest = ValidRequest with { Tag = otherUser.Tag };

            var request = CreateJsonRequest(HttpMethod.Patch, $"{Url}/{existingUser.Id}", invalidRequest);
            WithActorHeaders(request, existingUser);

            var response = await Client.SendAsync(request, TestContext.Current.CancellationToken);
            var problem = await ReadProblemDetailsAsync(response, TestContext.Current.CancellationToken);

            Assert.Equal(HttpStatusCode.UnprocessableEntity, response.StatusCode);
            Assert.NotNull(problem);
            Assert.Contains("Tag not available.", GetValidationErrors(problem, nameof(UpdateUserRequestModel.Tag)));
        }

        [Fact]
        public async Task ShouldReturnNoContent_WithPersistedChanges_WhenRequestIsValid()
        {
            var existingUser = SeedUsers[0];
            var updatedRequest = ValidRequest with { DisplayName = "Updated Display Name" };

            var request = CreateJsonRequest(HttpMethod.Patch, $"{Url}/{existingUser.Id}", updatedRequest);
            WithActorHeaders(request, existingUser);

            var response = await Client.SendAsync(request, TestContext.Current.CancellationToken);

            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

            var getRequest = CreateRequest(HttpMethod.Get, $"{Url}/{existingUser.Id}");
            var getResponse = await Client.SendAsync(getRequest, TestContext.Current.CancellationToken);
            var detail = await ReadContentAsync<UserDetailModel>(getResponse, TestContext.Current.CancellationToken);

            Assert.NotNull(detail);
            Assert.Equal(updatedRequest.DisplayName, detail.DisplayName);
        }
    }
}