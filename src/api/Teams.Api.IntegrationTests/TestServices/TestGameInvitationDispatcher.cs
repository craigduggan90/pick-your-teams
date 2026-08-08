using Teams.Core.Services.Invitations;
using Teams.Domain.Entities;

namespace Teams.Api.IntegrationTests.TestServices;

public class TestGameInvitationDispatcher : IGameInvitationDispatcher
{
    public record Invitation(string GameId, string EmailAddress, string? UserId);

    public List<Invitation> Invitations { get; } = [];

    public Task SendNewUserInvitationAsync(Game game, string emailAddress)
    {
        Invitations.Add(new Invitation(game.Id, emailAddress, null));
        return Task.CompletedTask;
    }

    public Task SendExistingUserInvitationAsync(Game game, string userId, string emailAddress)
    {
        Invitations.Add(new Invitation(game.Id, emailAddress, userId));
        return Task.CompletedTask;
    }
}