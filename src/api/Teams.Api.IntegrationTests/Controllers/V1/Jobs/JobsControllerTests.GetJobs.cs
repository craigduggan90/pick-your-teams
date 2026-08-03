// using Microsoft.Extensions.DependencyInjection;
// using System.Net;
// using Teams.Api.Controllers.V1.Jobs.ResponseModels;
// using Teams.Api.Infrastructure;
// using Teams.Common.Pagination;
// using Teams.Data.Context;
// using Teams.Domain.Entities;
// using Teams.Domain.Enums;
//
// namespace Teams.Api.IntegrationTests.Controllers.V1.Jobs;
//
// public static partial class JobsControllerTests
// {
//     public class GetJobs(ApiWebApplicationFactory factory) : JobsControllerTestsBase(factory)
//     {
//         [Fact]
//         public async Task ShouldReturnBadRequest_WhenVersionIsUnsupported()
//         {
//             var url = WithQuery(VersionlessUrl);
//             var request = CreateRequest(HttpMethod.Get, url, scopes: Scopes.Jobs.Read, apiVersion: "2.0");
//
//             var response = await Client.SendAsync(request, TestContext.Current.CancellationToken);
//
//             Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
//         }
//
//         [Fact]
//         public async Task ShouldReturnBadRequest_WhenVersionIsNotProvided()
//         {
//             var url = WithQuery(VersionlessUrl);
//             var request = CreateRequest(HttpMethod.Get, url, scopes: Scopes.Jobs.Read, apiVersion: null);
//
//             var response = await Client.SendAsync(request, TestContext.Current.CancellationToken);
//
//             Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
//         }
//
//         [Fact]
//         public async Task ShouldReturnForbidden_WhenScopeIsMissing()
//         {
//             var request = CreateRequest(HttpMethod.Get, Url);
//
//             var response = await Client.SendAsync(request, TestContext.Current.CancellationToken);
//
//             Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
//         }
//
//         [Fact]
//         public async Task ShouldReturnBadRequest_WhenPageSizeIsOutOfRange()
//         {
//             var url = WithQuery(Url, ("PageSize", "0"));
//             var request = CreateRequest(HttpMethod.Get, url, scopes: Scopes.Jobs.Read);
//
//             var response = await Client.SendAsync(request, TestContext.Current.CancellationToken);
//
//             Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
//         }
//
//         [Fact]
//         public async Task ShouldReturnOk_WithDefaultPageSize_WhenNoFiltersProvided()
//         {
//             var request = CreateRequest(HttpMethod.Get, Url, scopes: Scopes.Jobs.Read);
//
//             var response = await Client.SendAsync(request, TestContext.Current.CancellationToken);
//             var content =
//                 await ReadContentAsync<PagedList<JobResponseModel>>(response, TestContext.Current.CancellationToken);
//
//             Assert.Equal(HttpStatusCode.OK, response.StatusCode);
//             Assert.NotNull(content);
//             Assert.Equal(25, content.Data.Count);
//             Assert.Equal(25, content.Count);
//         }
//
//         [Fact]
//         public async Task ShouldReturnOk_WithPagedList_FilteredByType_WhenTypeProvided()
//         {
//             var url = WithQuery(Url, ("Type", nameof(JobTypeEnum.ArchiveProjectJob)), ("PageSize", "100"));
//             var request = CreateRequest(HttpMethod.Get, url, scopes: Scopes.Jobs.Read);
//
//             var response = await Client.SendAsync(request, TestContext.Current.CancellationToken);
//             var content =
//                 await ReadContentAsync<PagedList<JobResponseModel>>(response, TestContext.Current.CancellationToken);
//
//             Assert.Equal(HttpStatusCode.OK, response.StatusCode);
//             Assert.NotNull(content);
//             Assert.Equal(20, content.Data.Count); // 60 seed jobs, 3 types cycling evenly -> 20 each
//         }
//
//         [Fact]
//         public async Task ShouldReturnOk_WithPagedList_FilteredByStatus_WhenStatusProvided()
//         {
//             var url = WithQuery(Url, ("Status", nameof(JobStatusEnum.Complete)), ("PageSize", "100"));
//             var request = CreateRequest(HttpMethod.Get, url, scopes: Scopes.Jobs.Read);
//
//             var response = await Client.SendAsync(request, TestContext.Current.CancellationToken);
//             var content =
//                 await ReadContentAsync<PagedList<JobResponseModel>>(response, TestContext.Current.CancellationToken);
//
//             Assert.Equal(HttpStatusCode.OK, response.StatusCode);
//             Assert.NotNull(content);
//             Assert.Equal(15, content.Data.Count); // 60 seed jobs, 4 statuses cycling evenly -> 15 each
//         }
//
//         [Fact]
//         public async Task ShouldReturnOk_WithPagedList_FilteredByErrorCode_WhenErrorCodeProvided()
//         {
//             // Seed data never sets an ErrorCode, so a job with one is arranged specifically for this test.
//             const string errorCode = "FILTER_TEST_ERROR";
//             var job = new Job(Guid.NewGuid().ToString(), JobTypeEnum.ArchiveProjectJob, parameters: null);
//             job.Update(JobStatusEnum.Failed, errorCode, "Seeded for the ErrorCode filter test.");
//
//             await using (var scope = Factory.Services.CreateAsyncScope())
//             {
//                 var context = scope.ServiceProvider.GetRequiredService<ApiDbContext>();
//                 await context.Jobs.AddAsync(job, TestContext.Current.CancellationToken);
//                 await context.SaveChangesAsync(TestContext.Current.CancellationToken);
//             }
//
//             var url = WithQuery(Url, ("ErrorCode", errorCode));
//             var request = CreateRequest(HttpMethod.Get, url, scopes: Scopes.Jobs.Read);
//
//             var response = await Client.SendAsync(request, TestContext.Current.CancellationToken);
//             var content =
//                 await ReadContentAsync<PagedList<JobResponseModel>>(response, TestContext.Current.CancellationToken);
//
//             Assert.Equal(HttpStatusCode.OK, response.StatusCode);
//             Assert.NotNull(content);
//             Assert.Equal([job.Id], content.Data.Select(j => j.Id));
//         }
//
//         [Fact]
//         public async Task ShouldReturnOk_WithPagedList_FilteredByCreatedFrom_WhenCreatedFromProvided()
//         {
//             var cutoff = SeedJobs[29].DateCreated; // the 30th seeded job
//
//             var url = WithQuery(Url, ("CreatedFrom", cutoff.ToString("O")), ("PageSize", "100"));
//             var request = CreateRequest(HttpMethod.Get, url, scopes: Scopes.Jobs.Read);
//
//             var response = await Client.SendAsync(request, TestContext.Current.CancellationToken);
//             var content =
//                 await ReadContentAsync<PagedList<JobResponseModel>>(response, TestContext.Current.CancellationToken);
//
//             Assert.Equal(HttpStatusCode.OK, response.StatusCode);
//             Assert.NotNull(content);
//             Assert.Equal(31, content.Data.Count); // inclusive: jobs 30 through 60
//         }
//
//         [Fact]
//         public async Task ShouldReturnOk_WithPagedList_FilteredByCreatedTo_WhenCreatedToProvided()
//         {
//             var cutoff = SeedJobs[29].DateCreated; // the 30th seeded job
//
//             var url = WithQuery(Url, ("CreatedTo", cutoff.ToString("O")), ("PageSize", "100"));
//             var request = CreateRequest(HttpMethod.Get, url, scopes: Scopes.Jobs.Read);
//
//             var response = await Client.SendAsync(request, TestContext.Current.CancellationToken);
//             var content =
//                 await ReadContentAsync<PagedList<JobResponseModel>>(response, TestContext.Current.CancellationToken);
//
//             Assert.Equal(HttpStatusCode.OK, response.StatusCode);
//             Assert.NotNull(content);
//             Assert.Equal(29, content.Data.Count); // exclusive: jobs 1 through 29
//         }
//
//         [Fact]
//         public async Task ShouldReturnOk_WithPagedList_FilteredByModifiedFrom_WhenModifiedFromProvided()
//         {
//             // Seed data sets DateModified equal to DateCreated for every job, so this mirrors the CreatedFrom test.
//             var cutoff = SeedJobs[29].DateCreated;
//
//             var url = WithQuery(Url, ("ModifiedFrom", cutoff.ToString("O")), ("PageSize", "100"));
//             var request = CreateRequest(HttpMethod.Get, url, scopes: Scopes.Jobs.Read);
//
//             var response = await Client.SendAsync(request, TestContext.Current.CancellationToken);
//             var content =
//                 await ReadContentAsync<PagedList<JobResponseModel>>(response, TestContext.Current.CancellationToken);
//
//             Assert.Equal(HttpStatusCode.OK, response.StatusCode);
//             Assert.NotNull(content);
//             Assert.Equal(31, content.Data.Count);
//         }
//
//         [Fact]
//         public async Task ShouldReturnOk_WithPagedList_FilteredByModifiedTo_WhenModifiedToProvided()
//         {
//             var cutoff = SeedJobs[29].DateCreated;
//
//             var url = WithQuery(Url, ("ModifiedTo", cutoff.ToString("O")), ("PageSize", "100"));
//             var request = CreateRequest(HttpMethod.Get, url, scopes: Scopes.Jobs.Read);
//
//             var response = await Client.SendAsync(request, TestContext.Current.CancellationToken);
//             var content =
//                 await ReadContentAsync<PagedList<JobResponseModel>>(response, TestContext.Current.CancellationToken);
//
//             Assert.Equal(HttpStatusCode.OK, response.StatusCode);
//             Assert.NotNull(content);
//             Assert.Equal(29, content.Data.Count);
//         }
//
//         [Fact]
//         public async Task ShouldReturnOk_WithPagedList_FilteredByCursor_WhenCursorProvided()
//         {
//             var firstPageUrl = WithQuery(Url, ("PageSize", "10"));
//             var firstPageRequest = CreateRequest(HttpMethod.Get, firstPageUrl, scopes: Scopes.Jobs.Read);
//             var firstPageResponse = await Client.SendAsync(firstPageRequest, TestContext.Current.CancellationToken);
//             var firstPage =
//                 await ReadContentAsync<PagedList<JobResponseModel>>(firstPageResponse,
//                     TestContext.Current.CancellationToken);
//
//             Assert.NotNull(firstPage);
//             Assert.NotNull(firstPage.Cursor);
//
//             var secondPageUrl = WithQuery(Url, ("PageSize", "10"), ("Cursor", firstPage.Cursor));
//             var secondPageRequest = CreateRequest(HttpMethod.Get, secondPageUrl, scopes: Scopes.Jobs.Read);
//             var secondPageResponse = await Client.SendAsync(secondPageRequest, TestContext.Current.CancellationToken);
//             var secondPage =
//                 await ReadContentAsync<PagedList<JobResponseModel>>(secondPageResponse,
//                     TestContext.Current.CancellationToken);
//
//             Assert.Equal(HttpStatusCode.OK, secondPageResponse.StatusCode);
//             Assert.NotNull(secondPage);
//             Assert.Equal(10, secondPage.Data.Count);
//             Assert.Empty(firstPage.Data.Select(j => j.Id).Intersect(secondPage.Data.Select(j => j.Id)));
//         }
//
//         [Fact]
//         public async Task ShouldReturnOk_WithPagedList_FilteredByPageSize_WhenPageSizeProvided()
//         {
//             var url = WithQuery(Url, ("PageSize", "5"));
//             var request = CreateRequest(HttpMethod.Get, url, scopes: Scopes.Jobs.Read);
//
//             var response = await Client.SendAsync(request, TestContext.Current.CancellationToken);
//             var content =
//                 await ReadContentAsync<PagedList<JobResponseModel>>(response, TestContext.Current.CancellationToken);
//
//             Assert.Equal(HttpStatusCode.OK, response.StatusCode);
//             Assert.NotNull(content);
//             Assert.Equal(5, content.Data.Count);
//             Assert.Equal(5, content.Count);
//         }
//     }
// }