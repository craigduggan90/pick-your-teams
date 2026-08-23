using System.Net;
using Teams.Api.Controllers.V1.Users.RequestModels;
using Teams.Api.Controllers.V1.Users.ResponseModels;
using Teams.Api.Infrastructure;

namespace Teams.Api.IntegrationTests.Controllers.V1.Users;

public static partial class UsersControllerTests
{
    public class CreateUser(ApiWebApplicationFactory factory) : UsersControllerTestsBase(factory)
    {
        private static CreateUserRequestModel ValidRequest => new(
            DisplayName: "Jane Smith",
            ExternalId: $"external-{Guid.NewGuid():N}",
            Email: $"{Guid.NewGuid():N}@test.net",
            Mobile: "+447700900123");

        [Fact]
        public async Task ShouldReturnForbidden_WhenScopeIsMissing()
        {
            var request = CreateJsonRequest(HttpMethod.Post, Url, ValidRequest);

            var response = await Client.SendAsync(request, TestContext.Current.CancellationToken);

            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        }

        [Fact]
        public async Task ShouldReturnBadRequest_WhenVersionIsUnsupported()
        {
            var request = CreateJsonRequest(HttpMethod.Post, VersionlessUrl, ValidRequest, scopes: Scopes.Authoriser, apiVersion: "2.0");

            var response = await Client.SendAsync(request, TestContext.Current.CancellationToken);

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task ShouldReturnBadRequest_WhenVersionIsNotProvided()
        {
            var request = CreateJsonRequest(HttpMethod.Post, VersionlessUrl, ValidRequest, scopes: Scopes.Authoriser, apiVersion: null);

            var response = await Client.SendAsync(request, TestContext.Current.CancellationToken);

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task ShouldReturnUnprocessableEntity_WhenDisplayNameIsMissing()
        {
            var invalidRequest = ValidRequest with { DisplayName = "" };

            var request = CreateJsonRequest(HttpMethod.Post, Url, invalidRequest, scopes: Scopes.Authoriser);

            var response = await Client.SendAsync(request, TestContext.Current.CancellationToken);
            var problem = await ReadProblemDetailsAsync(response, TestContext.Current.CancellationToken);

            Assert.Equal(HttpStatusCode.UnprocessableEntity, response.StatusCode);
            Assert.NotNull(problem);
            Assert.NotEmpty(GetValidationErrors(problem, nameof(CreateUserRequestModel.DisplayName)));
        }

        [Fact]
        public async Task ShouldReturnUnprocessableEntity_WhenEmailIsInvalid()
        {
            var invalidRequest = ValidRequest with { Email = "not-an-email-address" };

            var request = CreateJsonRequest(HttpMethod.Post, Url, invalidRequest, scopes: Scopes.Authoriser);

            var response = await Client.SendAsync(request, TestContext.Current.CancellationToken);
            var problem = await ReadProblemDetailsAsync(response, TestContext.Current.CancellationToken);

            Assert.Equal(HttpStatusCode.UnprocessableEntity, response.StatusCode);
            Assert.NotNull(problem);
            Assert.NotEmpty(GetValidationErrors(problem, nameof(CreateUserRequestModel.Email)));
        }

        [Fact]
        public async Task ShouldReturnUnprocessableEntity_WhenEmailIsAlreadyInUse()
        {
            var existingUser = SeedUsers[0];
            var invalidRequest = ValidRequest with { Email = existingUser.EmailAddress };

            var request = CreateJsonRequest(HttpMethod.Post, Url, invalidRequest, scopes: Scopes.Authoriser);

            var response = await Client.SendAsync(request, TestContext.Current.CancellationToken);
            var problem = await ReadProblemDetailsAsync(response, TestContext.Current.CancellationToken);

            Assert.Equal(HttpStatusCode.UnprocessableEntity, response.StatusCode);
            Assert.NotNull(problem);
            Assert.NotEmpty(GetValidationErrors(problem, nameof(CreateUserRequestModel.Email)));
        }

        [Fact]
        public async Task ShouldReturnCreated_WithUserContent_WhenRequestIsValid()
        {
            var validRequest = ValidRequest;

            var request = CreateJsonRequest(HttpMethod.Post, Url, validRequest, scopes: Scopes.Authoriser);

            var response = await Client.SendAsync(request, TestContext.Current.CancellationToken);
            var content = await ReadContentAsync<UserModel>(response, TestContext.Current.CancellationToken);

            Assert.Equal(HttpStatusCode.Created, response.StatusCode);
            Assert.NotNull(content);
            Assert.Equal(validRequest.DisplayName, content.DisplayName);
            Assert.Equal(1000, content.Rating);
            Assert.EndsWith($"/api/v1/users/{content.Id}", response.Headers.Location?.ToString(), StringComparison.OrdinalIgnoreCase);
        }
    }
}