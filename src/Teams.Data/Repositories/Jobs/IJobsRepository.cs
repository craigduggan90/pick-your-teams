using Teams.Domain.Entities;

namespace Teams.Data.Repositories.Jobs;

/// <summary>Describes a read-write repository containing instances of <see cref="Job"/>.</summary>
public interface IJobsRepository : IReadWriteRepository<Job>, IReadOnlyJobsRepository;