using Teams.Data.Context;
using Teams.Domain.Entities;

namespace Teams.Data.Repositories.Players;

/// <inhertidoc />
public class PlayersRepository(ApiDbContext context)
    : ReadOnlyPlayersRepository(context), IPlayersRepository
{
    /// <inheritdoc />
    public async Task<Player> CreateAsync(Player entity, CancellationToken cancellationToken)
    {
        await Context.Players.AddAsync(entity, cancellationToken);
        return entity;
    }

    /// <inheritdoc />
    public Task<Player> UpdateAsync(Player entity, CancellationToken cancellationToken)
    {
        Context.Players.Update(entity);
        return Task.FromResult(entity);
    }

    /// <inheritdoc />
    public Task<Player> DeleteAsync(Player entity, CancellationToken cancellationToken)
    {
        Context.Players.Remove(entity);
        return Task.FromResult(entity);
    }
}