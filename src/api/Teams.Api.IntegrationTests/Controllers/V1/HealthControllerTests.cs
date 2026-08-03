using System.Net;

namespace Teams.Api.IntegrationTests.Controllers.V1;

public static class HealthControllerTests
{
    private const string BaseUrl = "api/health";
    private const string VersionedUrl = "api/v1/Health";

    public class Ping(ApiWebApplicationFactory factory) : ApiControllerTestsBase(factory)
    {
        [Fact]
        public async Task ShouldReturnNoContent_WhenCalledWithVersionHeader()
        {
            const HttpStatusCode expectedStatusCode = HttpStatusCode.NoContent;
            var request = CreateRequest(HttpMethod.Get, BaseUrl, apiVersion: "1.0");
            var response = await Client.SendAsync(request, TestContext.Current.CancellationToken);
            Assert.Equal(expectedStatusCode, response.StatusCode);
        }

        [Fact]
        public async Task ShouldReturnNoContent_WhenCalledWithVersionedUrl()
        {
            const HttpStatusCode expectedStatusCode = HttpStatusCode.NoContent;
            var request = CreateRequest(HttpMethod.Get, VersionedUrl);
            var response = await Client.SendAsync(request, TestContext.Current.CancellationToken);
            Assert.Equal(expectedStatusCode, response.StatusCode);
        }

        [Fact]
        public async Task ShouldReturnBadRequest_WhenCalledWithConflictingVersions()
        {
            const HttpStatusCode expectedStatusCode = HttpStatusCode.BadRequest;
            var request = CreateRequest(HttpMethod.Get, VersionedUrl, apiVersion: "0.1");
            var response = await Client.SendAsync(request, TestContext.Current.CancellationToken);
            Assert.Equal(expectedStatusCode, response.StatusCode);
        }
    }
}