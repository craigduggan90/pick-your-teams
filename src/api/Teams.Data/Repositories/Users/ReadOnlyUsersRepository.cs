using Microsoft.EntityFrameworkCore;
using Teams.Data.Context;
using Teams.Data.Filters;
using Teams.Data.Models;
using Teams.Domain.Entities;

namespace Teams.Data.Repositories.Users;

/// <inheritdoc />
public class ReadOnlyUsersRepository(ApiDbContext context)
    : RepositoryBase(context), IReadOnlyUsersRepository
{
    /// <inheritdoc />
    public async Task<User?> GetByIdAsync(string id, CancellationToken cancellationToken) =>
        await Context.Users.SingleOrDefaultAsync(entity => entity.Id == id, cancellationToken);

    /// <inheritdoc />
    public async Task<User?> GetByTagAsync(string tag, CancellationToken cancellationToken) =>
        await Context.Users.SingleOrDefaultAsync(entity => entity.Tag == tag, cancellationToken);

    /// <inheritdoc />
    public async Task<User?> GetByExternalIdAsync(string externalId, CancellationToken cancellationToken) =>
        await Context.Users.SingleOrDefaultAsync(entity => entity.ExternalId == externalId, cancellationToken);

    /// <inheritdoc />
    public async Task<User?> GetByEmailAddressAsync(string emailAddress, CancellationToken cancellationToken) =>
        await Context.Users.SingleOrDefaultAsync(entity => entity.EmailAddress == emailAddress, cancellationToken);

    /// <inheritdoc />
    public async Task<User?> GetByPhoneNumberAsync(string mobile, CancellationToken cancellationToken) =>
        await Context.Users.SingleOrDefaultAsync(entity => entity.Mobile == mobile, cancellationToken);

    /// <inheritdoc />
    public async Task<IEnumerable<User>> GetAsync(
        string? emailAddress,
        string? tag,
        string? displayName,
        RangeFilter<int>? rating,
        DateFilter? dateFilter = null,
        PaginationFilter? pagination = null,
        CancellationToken cancellationToken = default) =>
        await Context.Users
            .ApplyEmailAddressFilter(emailAddress)
            .ApplyTagFilter(tag)
            .ApplyDisplayNameFilter(displayName)
            .ApplyRatingFilter(rating)
            .ApplyBaseEntityDateFilters(dateFilter)
            .ApplyCursor(pagination?.Cursor)
            .ApplyPagination(pagination?.PageSize ?? Constants.DefaultPageSize)
            .ToListAsync(cancellationToken);
}