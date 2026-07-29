using Teams.Core.Exceptions;
using Teams.Core.Services.Jobs.Requests;
using Teams.Domain.Entities;

namespace Teams.Core.UnitTests.Services.Jobs;

public static partial class JobsServiceTests
{
    public class UpdateJobAsync : JobsServiceTestsBase
    {
        [Fact]
        public async Task ValidatesRequest_BeforeUpdating()
        {
            var sut = CreateSut();
            var job = CreateJob();
            JobsRepository.GetByIdAsync(job.Id, Arg.Any<CancellationToken>()).Returns(job);
            var request = new UpdateJobRequest(job.Id, job.ConcurrencyToken, "InProgress", null, null);

            await sut.UpdateJobAsync(request, TestContext.Current.CancellationToken);

            await Validator.Received(1).ValidateCommandAsync(request, Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task PropagatesValidationException_WhenValidationFails()
        {
            var sut = CreateSut();
            var request = new UpdateJobRequest("job-id", ConcurrencyToken, "InProgress", null, null);
            Validator.ValidateCommandAsync(request, Arg.Any<CancellationToken>())
                .Returns(Task.FromException(new CommandValidationException([])));

            await Assert.ThrowsAsync<CommandValidationException>(() =>
                sut.UpdateJobAsync(request, TestContext.Current.CancellationToken));
        }

        [Fact]
        public async Task ThrowsNotFoundException_WhenJobDoesNotExist()
        {
            var sut = CreateSut();
            var request = new UpdateJobRequest("missing-id", ConcurrencyToken, "InProgress", null, null);
            JobsRepository.GetByIdAsync("missing-id", Arg.Any<CancellationToken>()).Returns((Job?)null);

            await Assert.ThrowsAsync<NotFoundException>(() =>
                sut.UpdateJobAsync(request, TestContext.Current.CancellationToken));
        }

        [Fact]
        public async Task ThrowsConcurrencyTokenMismatchException_WhenTokenDoesNotMatchJob()
        {
            var sut = CreateSut();
            var job = CreateJob();
            JobsRepository.GetByIdAsync(job.Id, Arg.Any<CancellationToken>()).Returns(job);
            var request = new UpdateJobRequest(job.Id, ConcurrencyToken, "InProgress", null, null);

            await Assert.ThrowsAsync<ConcurrencyTokenMismatchException>(() =>
                sut.UpdateJobAsync(request, TestContext.Current.CancellationToken));
        }

        [Fact]
        public async Task DoesNotThrowConcurrencyTokenMismatchException_WhenTokenMatchesJob()
        {
            var sut = CreateSut();
            var job = CreateJob();
            JobsRepository.GetByIdAsync(job.Id, Arg.Any<CancellationToken>()).Returns(job);
            var request = new UpdateJobRequest(job.Id, job.ConcurrencyToken, "InProgress", null, null);

            var exception = await Record.ExceptionAsync(() =>
                sut.UpdateJobAsync(request, TestContext.Current.CancellationToken));

            Assert.Null(exception);
        }

        [Fact]
        public async Task DoesNotSave_WhenJobHasNotChanged()
        {
            var sut = CreateSut();
            var job = CreateJob();
            JobsRepository.GetByIdAsync(job.Id, Arg.Any<CancellationToken>()).Returns(job);
            var request = new UpdateJobRequest(job.Id, job.ConcurrencyToken, "Pending", null, null);

            await sut.UpdateJobAsync(request, TestContext.Current.CancellationToken);

            await JobsRepository.DidNotReceive().UpdateAsync(Arg.Any<Job>(), Arg.Any<CancellationToken>());
            await UnitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task ReturnsCurrentState_WhenJobHasNotChanged()
        {
            var sut = CreateSut();
            var job = CreateJob();
            JobsRepository.GetByIdAsync(job.Id, Arg.Any<CancellationToken>()).Returns(job);
            var request = new UpdateJobRequest(job.Id, job.ConcurrencyToken, "Pending", null, null);

            var result = await sut.UpdateJobAsync(request, TestContext.Current.CancellationToken);

            Assert.Equal(job.Id, result.Id);
            Assert.Equal("Pending", result.Status);
        }
    }
}