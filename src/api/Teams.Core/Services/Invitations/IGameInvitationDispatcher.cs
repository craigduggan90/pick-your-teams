using Teams.Domain.Entities;

namespace Teams.Core.Services.Invitations;

public interface IGameInvitationDispatcher
{
    Task SendNewUserInvitationAsync(Game game, string emailAddress);

    Task SendExistingUserInvitationAsync(Game game, string userId, string emailAddress);
}