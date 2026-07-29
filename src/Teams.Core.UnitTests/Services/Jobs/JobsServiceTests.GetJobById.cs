using Teams.Core.Exceptions;
using Teams.Domain.Entities;

namespace Teams.Core.UnitTests.Services.Jobs;

public static partial class JobsServiceTests
{
    public class GetJobByIdAsync : JobsServiceTestsBase
    {
        [Fact]
        public async Task ReturnsMappedJob_WhenJobExists()
        {
            var sut = CreateSut();
            var job = CreateJob();
            Repository.GetByIdAsync(job.Id, Arg.Any<CancellationToken>()).Returns(job);

            var result = await sut.GetJobByIdAsync(job.Id, TestContext.Current.CancellationToken);

            Assert.Equal(job.Id, result.Id);
        }

        [Fact]
        public async Task ThrowsNotFoundException_WhenJobDoesNotExist()
        {
            var sut = CreateSut();
            Repository.GetByIdAsync("missing-id", Arg.Any<CancellationToken>()).Returns((Job?)null);

            await Assert.ThrowsAsync<NotFoundException>(() =>
                sut.GetJobByIdAsync("missing-id", TestContext.Current.CancellationToken));
        }
    }
}