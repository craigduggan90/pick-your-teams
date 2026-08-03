namespace Teams.Data.Repositories;

/// <summary>Describes a generic, read/write repository.</summary>
/// <typeparam name="TEntity">The type of entity held in the repository.</typeparam>
public interface IReadWriteRepository<TEntity>
{
    /// <summary>Adds an instance of <typeparamref name="TEntity"/> to the current database context.</summary>
    /// <param name="entity">The entity to create.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <remarks>This method does not commit changes to the database.</remarks>
    Task<TEntity> CreateAsync(TEntity entity, CancellationToken cancellationToken);

    /// <summary>Updates an instance of <typeparamref name="TEntity"/> in the current database context.</summary>
    /// <param name="entity">The entity to update.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <remarks>This method does not commit changes to the database.</remarks>
    Task<TEntity> UpdateAsync(TEntity entity, CancellationToken cancellationToken);

    /// <summary>Removes an instance of <typeparamref name="TEntity"/> from the current database context.</summary>
    /// <param name="entity">The entity to delete.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <remarks>This method does not commit changes to the database.</remarks>
    Task<TEntity> DeleteAsync(TEntity entity, CancellationToken cancellationToken);
}