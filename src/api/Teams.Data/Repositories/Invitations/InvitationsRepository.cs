using Teams.Data.Context;
using Teams.Domain.Entities;

namespace Teams.Data.Repositories.Invitations;

public class InvitationsRepository(ApiDbContext context)
    : ReadOnlyInvitationsRepository(context), IInvitationsRepository
{
    /// <inheritdoc />
    public async Task<Invitation> CreateAsync(Invitation entity, CancellationToken cancellationToken)
    {
        await Context.Invitations.AddAsync(entity, cancellationToken);
        return entity;
    }

    /// <inheritdoc />
    public Task<Invitation> UpdateAsync(Invitation entity, CancellationToken cancellationToken)
    {
        Context.Invitations.Update(entity);
        return Task.FromResult(entity);
    }

    /// <inheritdoc />
    public Task<Invitation> DeleteAsync(Invitation entity, CancellationToken cancellationToken)
    {
        Context.Invitations.Remove(entity);
        return Task.FromResult(entity);
    }
}