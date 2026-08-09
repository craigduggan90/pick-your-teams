using Teams.Common.Providers.Temporal;
using Teams.Core.Services.Events;

namespace Teams.Core.UseCases.Invitations.CreateInvitations;

public record InvitationCreatedEvent(string Id, string GameId, string UserId) : IEvent
{
    public DateTime EventTime { get; } = DateTimeOffsetProvider.Now.UtcDateTime;

    public string Type => nameof(InvitationCreatedEvent);
}