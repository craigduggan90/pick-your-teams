using Microsoft.Extensions.Logging;
using System.Diagnostics.CodeAnalysis;

namespace Teams.Core.Services.Events;

[ExcludeFromCodeCoverage(Justification = "This is a stub, and will be replaced with a queue writer later.")]
public class EventPublisher(ILogger<EventPublisher> logger) : IEventPublisher
{
    public Task PublishEventAsync(IEvent @event, CancellationToken cancellationToken)
    {
        logger.LogInformation("Event published: {event}", @event);
        return Task.CompletedTask;
    }

    public async Task PublishEventsAsync(IEnumerable<IEvent> events, CancellationToken cancellationToken)
    {
        foreach (var @event in events)
            await PublishEventAsync(@event, cancellationToken);
    }
}