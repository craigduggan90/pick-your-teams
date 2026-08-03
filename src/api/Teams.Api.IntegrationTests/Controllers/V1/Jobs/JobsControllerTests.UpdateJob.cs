// using System.Net;
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
//     public class UpdateJob(ApiWebApplicationFactory factory) : JobsControllerTestsBase(factory)
//     {
//         private static UpdateJobRequestModel ValidRequest(string status) => new(status, ErrorCode: null, ErrorMessage: null);
//
//         [Fact]
//         public async Task ShouldReturnBadRequest_WhenVersionIsUnsupported()
//         {
//             var existingJob = SeedJobs[0];
//
//             var request = CreateJsonRequest(
//                 HttpMethod.Put,
//                 $"{VersionlessUrl}/{existingJob.Id}",
//                 ValidRequest(nameof(JobStatusEnum.Complete)),
//                 scopes: Scopes.Jobs.Modify,
//                 apiVersion: "2.0");
//             request.Headers.Add(Constants.IfMatchHeaderKey, ToETagValue(existingJob.ConcurrencyToken));
//
//             var response = await Client.SendAsync(request, TestContext.Current.CancellationToken);
//
//             Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
//         }
//
//         [Fact]
//         public async Task ShouldReturnBadRequest_WhenVersionIsNotProvided()
//         {
//             var existingJob = SeedJobs[0];
//
//             var request = CreateJsonRequest(
//                 HttpMethod.Put,
//                 $"{VersionlessUrl}/{existingJob.Id}",
//                 ValidRequest(nameof(JobStatusEnum.Complete)),
//                 scopes: Scopes.Jobs.Modify,
//                 apiVersion: null);
//             request.Headers.Add(Constants.IfMatchHeaderKey, ToETagValue(existingJob.ConcurrencyToken));
//
//             var response = await Client.SendAsync(request, TestContext.Current.CancellationToken);
//
//             Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
//         }
//
//         [Fact]
//         public async Task ShouldReturnForbidden_WhenScopeIsMissing()
//         {
//             var existingJob = SeedJobs[0];
//
//             var request = CreateJsonRequest(HttpMethod.Put, $"{Url}/{existingJob.Id}", ValidRequest(nameof(JobStatusEnum.Complete)));
//             request.Headers.Add(Constants.IfMatchHeaderKey, ToETagValue(existingJob.ConcurrencyToken));
//
//             var response = await Client.SendAsync(request, TestContext.Current.CancellationToken);
//
//             Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
//         }
//
//         [Fact]
//         public async Task ShouldReturnUnprocessableEntity_WhenStatusIsInvalid()
//         {
//             var existingJob = SeedJobs[0];
//
//             var request = CreateJsonRequest(HttpMethod.Put, $"{Url}/{existingJob.Id}", ValidRequest("NotARealStatus"), scopes: Scopes.Jobs.Modify);
//             request.Headers.Add(Constants.IfMatchHeaderKey, ToETagValue(existingJob.ConcurrencyToken));
//
//             var response = await Client.SendAsync(request, TestContext.Current.CancellationToken);
//
//             Assert.Equal(HttpStatusCode.UnprocessableEntity, response.StatusCode);
//         }
//
//         [Fact]
//         public async Task ShouldReturnPreconditionRequired_WhenIfMatchIsMissing()
//         {
//             var existingJob = SeedJobs[0];
//
//             var request = CreateJsonRequest(HttpMethod.Put, $"{Url}/{existingJob.Id}", ValidRequest(nameof(JobStatusEnum.Complete)), scopes: Scopes.Jobs.Modify);
//
//             var response = await Client.SendAsync(request, TestContext.Current.CancellationToken);
//
//             Assert.Equal(HttpStatusCode.PreconditionRequired, response.StatusCode);
//         }
//
//         [Fact]
//         public async Task ShouldReturnNotFound_WhenJobDoesNotExist()
//         {
//             var request = CreateJsonRequest(HttpMethod.Put, $"{Url}/does-not-exist", ValidRequest(nameof(JobStatusEnum.Complete)), scopes: Scopes.Jobs.Modify);
//             request.Headers.Add(Constants.IfMatchHeaderKey, ToETagValue("irrelevant-token"));
//
//             var response = await Client.SendAsync(request, TestContext.Current.CancellationToken);
//
//             Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
//         }
//
//         [Fact]
//         public async Task ShouldReturnPreconditionFailed_WhenConcurrencyTokenDoesNotMatch()
//         {
//             var existingJob = SeedJobs[0];
//
//             var request = CreateJsonRequest(HttpMethod.Put, $"{Url}/{existingJob.Id}", ValidRequest(nameof(JobStatusEnum.Complete)), scopes: Scopes.Jobs.Modify);
//             request.Headers.Add(Constants.IfMatchHeaderKey, ToETagValue("stale-token"));
//
//             var response = await Client.SendAsync(request, TestContext.Current.CancellationToken);
//
//             Assert.Equal(HttpStatusCode.PreconditionFailed, response.StatusCode);
//         }
//
//         [Fact]
//         public async Task ShouldReturnOk_WithUpdatedJobContent_WhenRequestIsValid()
//         {
//             var existingJob = SeedJobs[0];
//
//             var request = CreateJsonRequest(HttpMethod.Put, $"{Url}/{existingJob.Id}", ValidRequest(nameof(JobStatusEnum.Complete)), scopes: Scopes.Jobs.Modify);
//             request.Headers.Add(Constants.IfMatchHeaderKey, ToETagValue(existingJob.ConcurrencyToken));
//
//             var response = await Client.SendAsync(request, TestContext.Current.CancellationToken);
//             var content = await ReadContentAsync<JobResponseModel>(response, TestContext.Current.CancellationToken);
//
//             Assert.Equal(HttpStatusCode.OK, response.StatusCode);
//             Assert.NotNull(content);
//             Assert.Equal(existingJob.Id, content.Id);
//             Assert.Equal(nameof(JobStatusEnum.Complete), content.Status);
//             Assert.NotEqual(ToETagValue(existingJob.ConcurrencyToken), GetHeaderValues(response, Constants.ETagHeaderKey).SingleOrDefault());
//         }
//
//         [Fact]
//         public async Task ShouldReturnOk_WithUnchangedConcurrencyToken_WhenUpdateIsNoOp()
//         {
//             var existingJob = SeedJobs[0];
//
//             var request = CreateJsonRequest(HttpMethod.Put, $"{Url}/{existingJob.Id}", ValidRequest(existingJob.Status.ToString()), scopes: Scopes.Jobs.Modify);
//             request.Headers.Add(Constants.IfMatchHeaderKey, ToETagValue(existingJob.ConcurrencyToken));
//
//             var response = await Client.SendAsync(request, TestContext.Current.CancellationToken);
//
//             Assert.Equal(HttpStatusCode.OK, response.StatusCode);
//             Assert.Equal(ToETagValue(existingJob.ConcurrencyToken), GetHeaderValues(response, Constants.ETagHeaderKey).SingleOrDefault());
//         }
//     }
// }