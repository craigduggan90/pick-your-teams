using Teams.Common.Providers.Identifiers;
using Teams.Common.Providers.Temporal;
using Teams.Data.Repositories.Jobs;
using Teams.Domain.Entities;
using Teams.Domain.Enums;

namespace Teams.Data.UnitTests.Repositories.Jobs;

public static class JobsFilterHelperTests
{
    private static readonly DateTime BaseDate = new(2020, 1, 1, 12, 0, 0, DateTimeKind.Utc);
    private static readonly JobStatusEnum[] Statuses = [JobStatusEnum.Pending, JobStatusEnum.InProgress, JobStatusEnum.Complete, JobStatusEnum.Failed];
    private static readonly JobTypeEnum[] Types = [JobTypeEnum.ArchiveProjectJob, JobTypeEnum.ArchiveUserGroupJob];

    private static IQueryable<Job> GetSeedData(int count) => Enumerable.Range(1, count)
        .Select(i =>
        {
            using var idFix = new IdentifierProviderContext($"{i:D3}");
            using var dtFix = new DateTimeOffsetProviderContext(BaseDate.AddDays(i - 1));

            var job = new Job($"idempotency-key-{i:D3}", Types[i % Types.Length], null);
            var status = Statuses[i % Statuses.Length];
            var errorCode = status == JobStatusEnum.Failed ? $"ERR-{i:D3}" : null;
            var errorMessage = status == JobStatusEnum.Failed ? $"error-message-{i:D3}" : null;

            job.Update(status, errorCode, errorMessage);
            return job;
        })
        .AsQueryable();

    public class ApplyTypeFilter
    {
        [Fact]
        public void ShouldNotApplyFilter_WhenNoValueProvided()
        {
            var data = GetSeedData(30);
            var filtered = data.ApplyTypeFilter(null);
            Assert.Same(data, filtered);
        }

        [Fact]
        public void ShouldApplyFilter_WhenValueProvided()
        {
            const JobTypeEnum value = JobTypeEnum.ArchiveProjectJob;
            var data = GetSeedData(30);
            var expected = data.Where(job => job.Type == value);
            var filtered = data.ApplyTypeFilter(value);
            Assert.Equivalent(expected, filtered, true);
        }
    }

    public class ApplyStatusFilter
    {
        [Fact]
        public void ShouldNotApplyFilter_WhenNoValueProvided()
        {
            var data = GetSeedData(30);
            var filtered = data.ApplyStatusFilter(null);
            Assert.Same(data, filtered);
        }

        [Fact]
        public void ShouldApplyFilter_WhenValueProvided()
        {
            const JobStatusEnum value = JobStatusEnum.Failed;
            var data = GetSeedData(30);
            var expected = data.Where(job => job.Status == value);
            var filtered = data.ApplyStatusFilter(value);
            Assert.Equivalent(expected, filtered, true);
        }
    }

    public class ApplyErrorCodeFilter
    {
        [Fact]
        public void ShouldNotApplyFilter_WhenNoValueProvided()
        {
            var data = GetSeedData(30);
            var filtered = data.ApplyErrorCodeFilter(null);
            Assert.Same(data, filtered);
        }

        [Fact]
        public void ShouldApplyFilter_WhenValueProvided()
        {
            const string value = "ERR-004";
            var data = GetSeedData(30);
            var expected = data.Where(job => job.ErrorCode == value);
            var filtered = data.ApplyErrorCodeFilter(value);
            Assert.Equivalent(expected, filtered, true);
        }
    }
}