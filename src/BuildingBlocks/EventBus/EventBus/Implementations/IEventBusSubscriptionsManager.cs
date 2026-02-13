using static EventBus.Implementations.InMemoryEventBusSubscriptionsManager;

namespace EventBus.Implementations;

public interface IEventBusSubscriptionsManager
{
    bool IsEmpty { get; }
    event EventHandler<string>? OnEventRemoved;

    void AddSubscription<T, TH>() where T : Events.IntegrationEvent where TH : Abstractions.IIntegrationEventHandler<T>;
    void RemoveSubscription<T, TH>() where T : Events.IntegrationEvent where TH : Abstractions.IIntegrationEventHandler<T>;
    bool HasSubscriptionsForEvent<T>() where T : Events.IntegrationEvent;
    bool HasSubscriptionsForEvent(string eventName);
    Type? GetEventTypeByName(string eventName);
    void Clear();
    IEnumerable<SubscriptionInfo> GetHandlersForEvent<T>() where T : Events.IntegrationEvent;
    IEnumerable<SubscriptionInfo> GetHandlersForEvent(string eventName);
    string GetEventKey<T>();
}