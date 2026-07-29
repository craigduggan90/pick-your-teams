using Teams.Common.Providers.Identifiers;
using Teams.Common.Providers.Temporal;
using Teams.Data.UnitTests.TestHelpers;
using Teams.Domain.Entities;
using Teams.Domain.Enums;

namespace Teams.Data.UnitTests.Repositories.Jobs;

public abstract class JobsRepositoryTestsBase : DatabaseAwareTestBase
{
    protected static readonly DateTime BaseDate = new(2020, 1, 1, 12, 0, 0, DateTimeKind.Utc);
    private static readonly JobStatusEnum[] Statuses = [JobStatusEnum.Pending, JobStatusEnum.InProgress, JobStatusEnum.Complete, JobStatusEnum.Failed];
    private static readonly JobTypeEnum[] Types = [JobTypeEnum.ArchiveProjectJob, JobTypeEnum.ArchiveUserJob, JobTypeEnum.ArchiveUserGroupJob];

    private readonly Job[] _jobs = Enumerable
        .Range(1, 30)
        .Select(i =>
        {
            using var idFix = new IdentifierProviderContext($"{i:D3}");
            using var dtFix = new DateTimeOffsetProviderContext(BaseDate.AddDays(i - 1));

            var type = Types[(i - 1) % Types.Length];
            var job = new Job($"idempotency-key-{i:D3}", type, null);

            var status = Statuses[(i - 1) % Statuses.Length];
            var errorCode = status == JobStatusEnum.Failed ? $"ERR-{i:D3}" : null;
            var errorMessage = status == JobStatusEnum.Failed ? $"error-message-{i:D3}" : null;
            var eventTime = BaseDate.AddDays(i - 1).AddYears(1);

            using var updateDtFix = new DateTimeOffsetProviderContext(eventTime);
            job.Update(status, errorCode, errorMessage);

            return job;
        })
        .ToArray();

    public override async ValueTask InitializeAsync()
    {
        await Context.Jobs.AddRangeAsync(_jobs);
        await Context.SaveChangesAsync();
    }
}