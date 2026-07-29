using Teams.Domain.Entities;
using Teams.Data.Context;

namespace Teams.Data.Repositories.Jobs;

/// <summary>A read-write repository containing instances of <see cref="Job"/>.</summary>
public class JobsRepository(ApiDbContext context)
    : ReadOnlyJobsRepository(context), IJobsRepository
{
    /// <inheritdoc />
    public async Task<Job> CreateAsync(Job entity, CancellationToken cancellationToken)
    {
        await Context.Jobs.AddAsync(entity, cancellationToken);
        return entity;
    }

    /// <inheritdoc />
    public Task<Job> UpdateAsync(Job entity, CancellationToken cancellationToken)
    {
        Context.Jobs.Update(entity);
        return Task.FromResult(entity);
    }

    /// <inheritdoc />
    public Task<Job> DeleteAsync(Job entity, CancellationToken cancellationToken)
    {
        Context.Jobs.Remove(entity);
        return Task.FromResult(entity);
    }
}