using Teams.Common.Providers.Temporal;
using Teams.Core.Services.Events;

namespace Teams.Core.UseCases.Invitations.DeclineInvitation;

public record InvitationDeclinedEvent(string Id) : IEvent
{
    public DateTime EventTime { get; } = DateTimeOffsetProvider.Now.UtcDateTime;

    public string Type => nameof(InvitationDeclinedEvent);
}