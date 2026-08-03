using Teams.Data.Models;
using Teams.Domain.Entities;

namespace Teams.Data.Repositories.Users;

/// <summary>Describes a read-only repository containing instances of <see cref="User"/>.</summary>
public interface IReadOnlyUsersRepository : IReadOnlyRepository<User>
{
    /// <summary>Get a <see cref="User"/> of by its tag.</summary>
    /// <param name="tag">The users tag.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    Task<User?> GetByTagAsync(string tag, CancellationToken cancellationToken);

    /// <summary>Get a <see cref="User"/> of by its IdP identifier.</summary>
    /// <param name="externalId">The users external identifier.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    Task<User?> GetByExternalIdAsync(string externalId, CancellationToken cancellationToken);

    /// <summary>Get a <see cref="User"/> of by its email address.</summary>
    /// <param name="emailAddress">The user email address.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    Task<User?> GetByEmailAddressAsync(string emailAddress, CancellationToken cancellationToken);

    /// <summary>Get a <see cref="User"/> of by its phone number.</summary>
    /// <param name="mobile">The users phone number.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    Task<User?> GetByPhoneNumberAsync(string mobile, CancellationToken cancellationToken);

    /// <summary>Get a collection of users matching the given filters.</summary>
    /// <param name="emailAddress">Limit results to those matching this email address.</param>
    /// <param name="tag">Limit results to those matching this tag.</param>
    /// <param name="displayName">Limit results to those matching this display name.</param>
    /// <param name="rating">Limit results to those matching this rating.</param>
    /// <param name="dateFilter">Limit results to those matching this date filter.</param>
    /// <param name="pagination">Limit results to a page matching this pagination filter.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    Task<IEnumerable<User>> GetAsync(
        string? emailAddress,
        string? tag,
        string? displayName,
        RangeFilter<int>? rating,
        DateFilter? dateFilter = null,
        PaginationFilter? pagination = null,
        CancellationToken cancellationToken = default);
}