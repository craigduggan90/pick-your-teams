// using Microsoft.EntityFrameworkCore;
// using Microsoft.Extensions.DependencyInjection;
// using Teams.Common.Providers.Identifiers;
// using Teams.Common.Providers.Temporal;
// using Teams.Data.Context;
// using Teams.Domain.Entities;
// using Teams.Domain.Enums;
//
// namespace Teams.Api.IntegrationTests.Controllers.V1.Jobs;
//
// public static partial class JobsControllerTests
// {
//     private const string Url = "api/v1/jobs";
//     private const string VersionlessUrl = "api/jobs";
//
//     public abstract class JobsControllerTestsBase(ApiWebApplicationFactory factory)
//         : ApiControllerTestsBase(factory), IAsyncLifetime
//     {
//         protected IReadOnlyList<Job> SeedJobs { get; } = Enumerable.Range(1, 60).Select(CreateSeedJob).ToList();
//
//         protected static readonly DateTimeOffset BaseDate = new(2026, 1, 1, 12, 0, 0, TimeSpan.Zero);
//
//         public virtual async ValueTask InitializeAsync()
//         {
//             await using var scope = Factory.Services.CreateAsyncScope();
//             var context = scope.ServiceProvider.GetRequiredService<ApiDbContext>();
//
//             await context.Jobs.ExecuteDeleteAsync(TestContext.Current.CancellationToken);
//             await context.Jobs.AddRangeAsync(SeedJobs, TestContext.Current.CancellationToken);
//             await context.SaveChangesAsync(TestContext.Current.CancellationToken);
//         }
//
//         public virtual ValueTask DisposeAsync()
//         {
//             GC.SuppressFinalize(this);
//             return ValueTask.CompletedTask;
//         }
//
//         private static Job CreateSeedJob(int index)
//         {
//             using var idFix = new IdentifierProviderContext($"{index:D3}");
//             using var dtFix = new DateTimeOffsetProviderContext(BaseDate.AddDays(index));
//             var job = new Job($"idemp-{index:D3}", GetJobType(index), null);
//             job.Update(GetJobStatus(index), null, null);
//             return job;
//         }
//
//         private static JobTypeEnum GetJobType(int index)
//         {
//             var options = Enum.GetValues<JobTypeEnum>();
//             return options.ElementAt(index % options.Length);
//         }
//
//         private static JobStatusEnum GetJobStatus(int index)
//         {
//             var options = Enum.GetValues<JobStatusEnum>();
//             return options.ElementAt(index % options.Length);
//         }
//     }
// }