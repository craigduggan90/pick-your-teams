namespace Teams.Core.Services.Events;

public interface IEvent
{
    string Id { get; }

    DateTime EventTime { get; }

    string Type { get; }
}