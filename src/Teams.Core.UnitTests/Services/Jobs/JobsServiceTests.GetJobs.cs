using Teams.Data.Models;
using Teams.Core.Exceptions;
using Teams.Core.Services.Jobs.Requests;
using Teams.Domain.Enums;

namespace Teams.Core.UnitTests.Services.Jobs;

public static partial class JobsServiceTests
{
    public class GetJobsAsync : JobsServiceTestsBase
    {
        [Fact]
        public async Task ValidatesRequest_BeforeQuerying()
        {
            var sut = CreateSut();
            var request = new GetJobsRequest();

            await sut.GetJobsAsync(request, TestContext.Current.CancellationToken);

            await Validator.Received(1).ValidateQueryAsync(request, Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task PropagatesValidationException_WhenValidationFails()
        {
            var sut = CreateSut();
            var request = new GetJobsRequest();
            Validator.ValidateQueryAsync(request, Arg.Any<CancellationToken>())
                .Returns(Task.FromException(new QueryValidationException([])));

            await Assert.ThrowsAsync<QueryValidationException>(() =>
                sut.GetJobsAsync(request, TestContext.Current.CancellationToken));
        }

        [Fact]
        public async Task ParsesTypeAndStatus_BeforeQueryingRepository()
        {
            var sut = CreateSut();
            var request = new GetJobsRequest(Type: "ArchiveProjectJob", Status: "Failed");
            Repository.GetAsync(
                    Arg.Any<JobTypeEnum?>(),
                    Arg.Any<JobStatusEnum?>(),
                    Arg.Any<string?>(),
                     Arg.Any<DateFilter>(),
                    Arg.Any<PaginationFilter>(),
                    Arg.Any<CancellationToken>())
                .Returns([]);

            await sut.GetJobsAsync(request, TestContext.Current.CancellationToken);

            await Repository.Received(1).GetAsync(
                JobTypeEnum.ArchiveProjectJob,
                JobStatusEnum.Failed,
                null,
                Arg.Any<DateFilter>(),
                Arg.Any<PaginationFilter>(),
                Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task ReturnsMappedPagedList_WhenJobsExist()
        {
            var sut = CreateSut();
            var request = new GetJobsRequest();
            var job = CreateJob();
            Repository.GetAsync(
                    Arg.Any<JobTypeEnum?>(),
                    Arg.Any<JobStatusEnum?>(),
                    Arg.Any<string?>(),
                    Arg.Any<DateFilter>(),
                    Arg.Any<PaginationFilter>(),
                    Arg.Any<CancellationToken>())
                .Returns([job]);

            var result = await sut.GetJobsAsync(request, TestContext.Current.CancellationToken);

            Assert.Single(result.Data);
            Assert.Equal(job.Id, result.Data[0].Id);
        }
    }
}