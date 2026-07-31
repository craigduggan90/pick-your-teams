using Microsoft.EntityFrameworkCore;
using Teams.Data.Context;
using Teams.Domain.Entities;

namespace Teams.Data.Repositories.Users;

/// <inheritdoc />
public class ReadOnlyUsersRepository(ApiDbContext context)
    : RepositoryBase(context), IReadOnlyUsersRepository
{
    /// <inheritdoc />
    public async Task<User?> GetByIdAsync(string id, CancellationToken cancellationToken) =>
        await Context.Users.SingleOrDefaultAsync(entity => entity.Id == id, cancellationToken);
}