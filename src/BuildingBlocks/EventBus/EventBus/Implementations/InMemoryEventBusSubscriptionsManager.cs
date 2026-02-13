using EventBus.Abstractions;

namespace EventBus.Implementations;

public partial class InMemoryEventBusSubscriptionsManager : IEventBusSubscriptionsManager
{
    private readonly Dictionary<string, List<SubscriptionInfo>> _handlers = new();
    private readonly List<Type> _eventTypes = new();

    public event EventHandler<string>? OnEventRemoved;
    public bool IsEmpty => _handlers.Count == 0;
    public void Clear() => _handlers.Clear();

    public void AddSubscription<T, TH>()
        where T : Events.IntegrationEvent
        where TH : IIntegrationEventHandler<T>
    {
        var eventName = GetEventKey<T>();
        DoAddSubscription(typeof(TH), eventName, isDynamic: false);
        if (!_eventTypes.Contains(typeof(T))) _eventTypes.Add(typeof(T));
    }

    private void DoAddSubscription(Type handlerType, string eventName, bool isDynamic)
    {
        if (!HasSubscriptionsForEvent(eventName)) _handlers.Add(eventName, new List<SubscriptionInfo>());
        if (_handlers[eventName].Any(s => s.HandlerType == handlerType))
            throw new ArgumentException($"Handler already registered for '{eventName}'", nameof(handlerType));
        _handlers[eventName].Add(isDynamic ? SubscriptionInfo.Dynamic(handlerType) : SubscriptionInfo.Typed(handlerType));
    }

    public void RemoveSubscription<T, TH>() where T : Events.IntegrationEvent where TH : IIntegrationEventHandler<T>
        => DoRemoveHandler(GetEventKey<T>(), FindSubscriptionToRemove<T, TH>());

    public IEnumerable<SubscriptionInfo> GetHandlersForEvent<T>() where T : Events.IntegrationEvent => GetHandlersForEvent(GetEventKey<T>());
    public IEnumerable<SubscriptionInfo> GetHandlersForEvent(string eventName) => _handlers[eventName];

    private void DoRemoveHandler(string eventName, SubscriptionInfo? subsToRemove)
    {
        if (subsToRemove == null) return;
        _handlers[eventName].Remove(subsToRemove);
        if (!_handlers[eventName].Any())
        {
            _handlers.Remove(eventName);
            var eventType = _eventTypes.SingleOrDefault(e => e.Name == eventName);
            if (eventType != null) _eventTypes.Remove(eventType);
            OnEventRemoved?.Invoke(this, eventName);
        }
    }

    private SubscriptionInfo? FindSubscriptionToRemove<T, TH>() where T : Events.IntegrationEvent where TH : IIntegrationEventHandler<T>
        => DoFindSubscriptionToRemove(GetEventKey<T>(), typeof(TH));

    private SubscriptionInfo? DoFindSubscriptionToRemove(string eventName, Type handlerType)
        => HasSubscriptionsForEvent(eventName) ? _handlers[eventName].SingleOrDefault(s => s.HandlerType == handlerType) : null;

    public bool HasSubscriptionsForEvent<T>() where T : Events.IntegrationEvent => HasSubscriptionsForEvent(GetEventKey<T>());
    public bool HasSubscriptionsForEvent(string eventName) => _handlers.ContainsKey(eventName);
    public Type? GetEventTypeByName(string eventName) => _eventTypes.SingleOrDefault(t => t.Name == eventName);
    public string GetEventKey<T>() => typeof(T).Name;
}