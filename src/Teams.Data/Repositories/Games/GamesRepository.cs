using Teams.Data.Context;
using Teams.Domain.Entities;

namespace Teams.Data.Repositories.Games;

/// <summary>A read-write repository containing instances of <see cref="Player"/>.</summary>
public class GamesRepository(ApiDbContext context)
    : ReadOnlyGamesRepository(context), IGamesRepository
{
    /// <inheritdoc />
    public async Task<Game> CreateAsync(Game entity, CancellationToken cancellationToken)
    {
        await Context.Games.AddAsync(entity, cancellationToken);
        return entity;
    }

    /// <inheritdoc />
    public Task<Game> UpdateAsync(Game entity, CancellationToken cancellationToken)
    {
        Context.Games.Update(entity);
        return Task.FromResult(entity);
    }

    /// <inheritdoc />
    public Task<Game> DeleteAsync(Game entity, CancellationToken cancellationToken)
    {
        Context.Games.Remove(entity);
        return Task.FromResult(entity);
    }
}