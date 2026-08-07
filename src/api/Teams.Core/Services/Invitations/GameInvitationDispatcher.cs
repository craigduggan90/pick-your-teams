using Microsoft.Extensions.Logging;
using System.Diagnostics.CodeAnalysis;
using Teams.Domain.Entities;

namespace Teams.Core.Services.Invitations;

// This is a stub that we'll implement later
[ExcludeFromCodeCoverage]
public class GameInvitationDispatcher(
    ILogger<GameInvitationDispatcher> logger) : IGameInvitationDispatcher
{
    public Task SendNewUserInvitationAsync(Game game, string emailAddress)
    {
        logger.LogInformation("Game invitation sent: {gameId} ({emailAddress})", game.Id, emailAddress);
        return Task.CompletedTask;
    }

    public Task SendExistingUserInvitationAsync(Game game, string userId, string emailAddress)
    {
        logger.LogInformation("Game invitation sent: {gameId} ({emailAddress} - '{userId}')", game.Id, emailAddress, userId);
        return Task.CompletedTask;
    }
}