using System;
using System.Text.Json.Serialization;

namespace EventBus.Events;

public record IntegrationEvent
{
    [JsonInclude]
    public Guid Id { get; private set; } = Guid.NewGuid();

    [JsonInclude]
    public DateTime CreationDate { get; private set; } = DateTime.UtcNow;

    [JsonInclude]
    public string EventType { get; private set; }

    public IntegrationEvent()
    {
        EventType = GetType().FullName!;
    }
}