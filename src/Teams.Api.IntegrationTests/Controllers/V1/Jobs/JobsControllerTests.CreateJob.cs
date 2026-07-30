// using System.Net;
// using System.Text.Json;
// using Teams.Api.Controllers.V1.Jobs.RequestModels;
// using Teams.Api.Controllers.V1.Jobs.ResponseModels;
// using Teams.Api.Infrastructure;
// using Teams.Common;
// using Teams.Domain.Enums;
//
// namespace Teams.Api.IntegrationTests.Controllers.V1.Jobs;
//
// public static partial class JobsControllerTests
// {
//     public class CreateJob(ApiWebApplicationFactory factory) : JobsControllerTestsBase(factory)
//     {
//         private static CreateJobRequestModel ValidRequest => new(nameof(JobTypeEnum.ArchiveProjectJob), Parameters: null);
//
//         [Fact]
//         public async Task ShouldReturnBadRequest_WhenVersionIsUnsupported()
//         {
//             var request = CreateJsonRequest(
//                 HttpMethod.Post,
//                 VersionlessUrl,
//                 ValidRequest,
//                 scopes: Scopes.Jobs.Enqueue,
//                 apiVersion: "2.0");
//             request.Headers.Add(Constants.IdempotencyHeaderKey, Guid.NewGuid().ToString());
//
//             var response = await Client.SendAsync(request, TestContext.Current.CancellationToken);
//
//             Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
//         }
//
//         [Fact]
//         public async Task ShouldReturnBadRequest_WhenVersionIsNotProvided()
//         {
//             var request = CreateJsonRequest(
//                 HttpMethod.Post,
//                 VersionlessUrl,
//                 ValidRequest,
//                 scopes: Scopes.Jobs.Enqueue,
//                 apiVersion: null);
//             request.Headers.Add(Constants.IdempotencyHeaderKey, Guid.NewGuid().ToString());
//
//             var response = await Client.SendAsync(request, TestContext.Current.CancellationToken);
//
//             Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
//         }
//
//         [Fact]
//         public async Task ShouldReturnForbidden_WhenScopeIsMissing()
//         {
//             var request = CreateJsonRequest(HttpMethod.Post, Url, ValidRequest);
//             request.Headers.Add(Constants.IdempotencyHeaderKey, Guid.NewGuid().ToString());
//
//             var response = await Client.SendAsync(request, TestContext.Current.CancellationToken);
//
//             Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
//         }
//
//         [Fact]
//         public async Task ShouldReturnUnprocessableEntity_WhenTypeIsInvalid()
//         {
//             var invalidRequest = ValidRequest with { Type = "NotARealJobType" };
//
//             var request = CreateJsonRequest(HttpMethod.Post, Url, invalidRequest, scopes: Scopes.Jobs.Enqueue);
//             request.Headers.Add(Constants.IdempotencyHeaderKey, Guid.NewGuid().ToString());
//
//             var response = await Client.SendAsync(request, TestContext.Current.CancellationToken);
//
//             Assert.Equal(HttpStatusCode.UnprocessableEntity, response.StatusCode);
//         }
//
//         [Fact]
//         public async Task ShouldReturnPreconditionRequired_WhenIdempotencyKeyIsMissing()
//         {
//             var request = CreateJsonRequest(HttpMethod.Post, Url, ValidRequest, scopes: Scopes.Jobs.Enqueue);
//
//             var response = await Client.SendAsync(request, TestContext.Current.CancellationToken);
//
//             Assert.Equal(HttpStatusCode.PreconditionRequired, response.StatusCode);
//         }
//
//         [Fact]
//         public async Task ShouldReturnAccepted_WithJobContent_WhenRequestIsValid()
//         {
//             var idempotencyKey = Guid.NewGuid().ToString();
//
//             var request = CreateJsonRequest(HttpMethod.Post, Url, ValidRequest, scopes: Scopes.Jobs.Enqueue);
//             request.Headers.Add(Constants.IdempotencyHeaderKey, idempotencyKey);
//
//             var response = await Client.SendAsync(request, TestContext.Current.CancellationToken);
//             var content = await ReadContentAsync<JobResponseModel>(response, TestContext.Current.CancellationToken);
//
//             Assert.Equal(HttpStatusCode.Accepted, response.StatusCode);
//             Assert.NotNull(content);
//             Assert.Equal(idempotencyKey, content.IdempotencyKey);
//             Assert.Equal(nameof(JobStatusEnum.Pending), content.Status);
//             Assert.Equal(ToETagValue(content.ConcurrencyToken), GetHeaderValues(response, Constants.ETagHeaderKey).SingleOrDefault());
//             Assert.EndsWith($"/api/v1/jobs/{content.Id}", response.Headers.Location?.ToString(), StringComparison.OrdinalIgnoreCase);
//         }
//
//         [Fact]
//         public async Task ShouldReturnAccepted_WithExistingJobContent_WhenIdempotencyKeyAlreadyExists()
//         {
//             var existingJob = SeedJobs[0];
//
//             var request = CreateJsonRequest(HttpMethod.Post, Url, ValidRequest, scopes: Scopes.Jobs.Enqueue);
//             request.Headers.Add(Constants.IdempotencyHeaderKey, existingJob.IdempotencyKey);
//
//             var response = await Client.SendAsync(request, TestContext.Current.CancellationToken);
//             var content = await ReadContentAsync<JobResponseModel>(response, TestContext.Current.CancellationToken);
//
//             Assert.Equal(HttpStatusCode.Accepted, response.StatusCode);
//             Assert.NotNull(content);
//             Assert.Equal(existingJob.Id, content.Id);
//             Assert.Equal(ToETagValue(existingJob.ConcurrencyToken), GetHeaderValues(response, Constants.ETagHeaderKey).SingleOrDefault());
//             Assert.EndsWith($"/api/v1/jobs/{content.Id}", response.Headers.Location?.ToString(), StringComparison.OrdinalIgnoreCase);
//         }
//
//         [Fact]
//         public async Task ShouldReturnAccepted_WithParametersPersisted_WhenParametersAreProvided()
//         {
//             var parameters = JsonSerializer.SerializeToElement(new { archiveReason = "seeded-for-test", retainDays = 30 });
//             var body = ValidRequest with { Parameters = parameters };
//
//             var createRequest = CreateJsonRequest(HttpMethod.Post, Url, body, scopes: Scopes.Jobs.Enqueue);
//             createRequest.Headers.Add(Constants.IdempotencyHeaderKey, Guid.NewGuid().ToString());
//
//             var createResponse = await Client.SendAsync(createRequest, TestContext.Current.CancellationToken);
//             var created = await ReadContentAsync<JobResponseModel>(createResponse, TestContext.Current.CancellationToken);
//
//             Assert.Equal(HttpStatusCode.Accepted, createResponse.StatusCode);
//             Assert.NotNull(created);
//
//             var getRequest = CreateRequest(HttpMethod.Get, $"{Url}/{created.Id}", scopes: Scopes.Jobs.Read);
//             var getResponse = await Client.SendAsync(getRequest, TestContext.Current.CancellationToken);
//             var detail = await ReadContentAsync<JobResponseDetailModel>(getResponse, TestContext.Current.CancellationToken);
//
//             Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);
//             Assert.NotNull(detail);
//             Assert.Equal(
//                 JsonSerializer.Serialize(parameters),
//                 JsonSerializer.Serialize((JsonElement)detail.Parameters!));
//         }
//     }
// }