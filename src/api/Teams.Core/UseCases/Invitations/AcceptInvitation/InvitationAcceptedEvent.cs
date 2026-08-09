using Teams.Common.Providers.Temporal;
using Teams.Core.Services.Events;

namespace Teams.Core.UseCases.Invitations.AcceptInvitation;

public record InvitationAcceptedEvent(string Id) : IEvent
{
    public DateTime EventTime { get; } = DateTimeOffsetProvider.Now.UtcDateTime;

    public string Type => nameof(InvitationAcceptedEvent);
}