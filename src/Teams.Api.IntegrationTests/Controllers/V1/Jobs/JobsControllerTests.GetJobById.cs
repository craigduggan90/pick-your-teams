using Teams.Api.Controllers.V1.Jobs.ResponseModels;
using Teams.Api.Infrastructure;
using Teams.Common;
using System.Net;

namespace Teams.Api.IntegrationTests.Controllers.V1.Jobs;

public static partial class JobsControllerTests
{
    public class GetJobById(ApiWebApplicationFactory factory) : JobsControllerTestsBase(factory)
    {
        [Fact]
        public async Task ShouldReturnBadRequest_WhenVersionIsUnsupported()
        {
            var existingJob = SeedJobs[0];

            var request = CreateRequest(
                HttpMethod.Get,
                $"{VersionlessUrl}/{existingJob.Id}",
                scopes: Scopes.Jobs.Read,
                apiVersion: "2.0");

            var response = await Client.SendAsync(request, TestContext.Current.CancellationToken);

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task ShouldReturnBadRequest_WhenVersionIsNotProvided()
        {
            var existingJob = SeedJobs[0];

            var request = CreateRequest(
                HttpMethod.Get,
                $"{VersionlessUrl}/{existingJob.Id}",
                scopes: Scopes.Jobs.Read,
                apiVersion: null);

            var response = await Client.SendAsync(request, TestContext.Current.CancellationToken);

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task ShouldReturnForbidden_WhenScopeIsMissing()
        {
            var existingJob = SeedJobs[0];

            var request = CreateRequest(HttpMethod.Get, $"{Url}/{existingJob.Id}");

            var response = await Client.SendAsync(request, TestContext.Current.CancellationToken);

            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        }

        [Fact]
        public async Task ShouldReturnNotFound_WhenJobDoesNotExist()
        {
            var request = CreateRequest(HttpMethod.Get, $"{Url}/does-not-exist", scopes: Scopes.Jobs.Read);

            var response = await Client.SendAsync(request, TestContext.Current.CancellationToken);

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task ShouldReturnOk_WithJobDetailContent_WhenJobExists()
        {
            var existingJob = SeedJobs[0];

            var request = CreateRequest(HttpMethod.Get, $"{Url}/{existingJob.Id}", scopes: Scopes.Jobs.Read);

            var response = await Client.SendAsync(request, TestContext.Current.CancellationToken);
            var content = await ReadContentAsync<JobResponseDetailModel>(response, TestContext.Current.CancellationToken);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.NotNull(content);
            Assert.Equal(existingJob.Id, content.Id);
            Assert.Equal(existingJob.IdempotencyKey, content.IdempotencyKey);
            Assert.Equal(existingJob.Type.ToString(), content.Type);
            Assert.Equal(existingJob.Status.ToString(), content.Status);
            Assert.Null(content.Error);
            Assert.Equal(ToETagValue(existingJob.ConcurrencyToken), GetHeaderValues(response, Constants.ETagHeaderKey).SingleOrDefault());
        }
    }
}