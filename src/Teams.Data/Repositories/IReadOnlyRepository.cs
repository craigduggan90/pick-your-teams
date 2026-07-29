namespace Teams.Data.Repositories;

/// <summary>Describes a generic, read-only repository.</summary>
/// <typeparam name="TEntity">The type of entity held in the repository.</typeparam>
public interface IReadOnlyRepository<TEntity>
{
    /// <summary>Get an instance of <typeparamref name="TEntity"/> by its unique identifier.</summary>
    /// <param name="id">The unique identifier of the instance to retrieve.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    Task<TEntity?> GetByIdAsync(string id, CancellationToken cancellationToken);
}