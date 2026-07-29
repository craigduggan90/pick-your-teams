using Teams.Core.Services.Jobs;
using Teams.Core.Services.Validation;
using Teams.Data.Repositories.Jobs;
using Teams.Data.Services;
using Teams.Domain.Entities;
using Teams.Domain.Enums;

namespace Teams.Core.UnitTests.Services.Jobs;

public static partial class JobsServiceTests
{
    public abstract class JobsServiceTestsBase
    {
        protected readonly IReadOnlyJobsRepository Repository = Substitute.For<IReadOnlyJobsRepository>();
        protected readonly IUnitOfWork UnitOfWork = Substitute.For<IUnitOfWork>();
        protected readonly IJobsRepository JobsRepository = Substitute.For<IJobsRepository>();
        protected readonly IValidationService Validator = Substitute.For<IValidationService>();

        protected const string ConcurrencyToken = "6a56255b5d61226bc0e680b3c4d29d42";

        protected JobsServiceTestsBase()
        {
            UnitOfWork.Jobs.Returns(JobsRepository);
            JobsRepository.CreateAsync(Arg.Any<Job>(), Arg.Any<CancellationToken>())
                .Returns(callInfo => Task.FromResult(callInfo.Arg<Job>()!));
        }

        protected JobsService CreateSut() => new(Repository, UnitOfWork, Validator);

        protected static Job CreateJob(
            string idempotencyKey = "idempotency-key-001",
            JobTypeEnum type = JobTypeEnum.ArchiveProjectJob,
            string? parameters = null)
            => new(idempotencyKey, type, parameters);
    }
}