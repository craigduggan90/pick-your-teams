using Teams.Domain.Entities;
using Teams.Data.Repositories.Jobs;

namespace Teams.Data.Services;

public interface IUnitOfWork
{
    /// <summary>Accessor for the <see cref="Job"/> repository.</summary>
    IJobsRepository Jobs { get; }

    /// <summary>Saves all changes made in this context to the database.</summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    Task SaveChangesAsync(CancellationToken cancellationToken);
}