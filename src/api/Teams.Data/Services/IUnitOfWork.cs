using Teams.Data.Context;
using Teams.Data.Repositories.Games;
using Teams.Data.Repositories.Invitations;
using Teams.Data.Repositories.Players;
using Teams.Data.Repositories.Users;
using Teams.Domain.Entities;

namespace Teams.Data.Services;

public interface IUnitOfWork
{
    ApiDbContext Context { get; }
    
    /// <summary>Accessor for the <see cref="Player"/> repository.</summary>
    IPlayersRepository Players { get; }

    /// <summary>Accessor for the <see cref="Game"/> repository.</summary>
    IGamesRepository Games { get; }

    /// <summary>Accessor for the <see cref="User"/> repository.</summary>
    IUsersRepository Users { get; }

    /// <summary>Accessor for the <see cref="Invitation"/> repository.</summary>
    IInvitationsRepository Invitations { get; }

    /// <summary>Saves all changes made in this context to the database.</summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    Task SaveChangesAsync(CancellationToken cancellationToken);
}