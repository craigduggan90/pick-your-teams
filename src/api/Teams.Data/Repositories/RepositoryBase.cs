using Teams.Data.Context;

namespace Teams.Data.Repositories;

/// <summary>Base class implemented by repository implementations.</summary>
/// <param name="context">The database context.</param>
public abstract class RepositoryBase(ApiDbContext context)
{
    /// <summary>The repository database context.</summary>
    protected ApiDbContext Context { get; } = context;
}