using Teams.Core.Services.Events;

namespace Teams.Api.EndToEndTests.TestServices;

public class TestEventPublisher : IEventPublisher
{
    public List<IEvent> Events { get; } = [];

    public IEvent? GetLatest() => Events.LastOrDefault();

    public TEvent? GetLatestAs<TEvent>() where TEvent : class, IEvent =>
        GetLatest() as TEvent;

    public Task PublishEventAsync(IEvent @event, CancellationToken cancellationToken)
    {
        Events.Add(@event);
        return Task.CompletedTask;
    }

    public Task PublishEventsAsync(IEnumerable<IEvent> events, CancellationToken cancellationToken)
    {
        Events.AddRange(events);
        return Task.CompletedTask;
    }
}