namespace Teams.Core.Services.Events;

public interface IEventPublisher
{
    Task PublishEventAsync(IEvent @event, CancellationToken cancellationToken);

    Task PublishEventsAsync(IEnumerable<IEvent> events, CancellationToken cancellationToken);
}