using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using EventBus.Abstractions;
using Microsoft.Extensions.Logging;
using Polly;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using RabbitMQ.Client.Exceptions;

namespace EventBus.Implementations;

public class RabbitMQEventBus : IEventBus, IDisposable
{
    private const string BROKER_NAME = "ecommerce_event_bus";
    private readonly IRabbitMQPersistentConnection _persistentConnection;
    private readonly ILogger<RabbitMQEventBus> _logger;
    private readonly IEventBusSubscriptionsManager _subsManager;
    private readonly IServiceProvider _serviceProvider;
    private readonly int _retryCount;
    private IModel? _consumerChannel;
    private string? _queueName;

    public RabbitMQEventBus(IRabbitMQPersistentConnection persistentConnection, ILogger<RabbitMQEventBus> logger,
        IServiceProvider serviceProvider, IEventBusSubscriptionsManager subsManager, string? queueName = null, int retryCount = 5)
    {
        _persistentConnection = persistentConnection;
        _logger = logger;
        _subsManager = subsManager;
        _queueName = queueName;
        _serviceProvider = serviceProvider;
        _retryCount = retryCount;
        _consumerChannel = CreateConsumerChannel();
    }

    public Task PublishAsync(Events.IntegrationEvent @event)
    {
        if (!_persistentConnection.IsConnected) _persistentConnection.TryConnect();

        var policy = Policy.Handle<BrokerUnreachableException>().Or<SocketException>()
            .WaitAndRetry(_retryCount, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),
                (ex, time) => _logger.LogWarning(ex, "Could not publish event after {Timeout}s", $"{time.TotalSeconds:n1}"));

        var eventName = @event.GetType().Name;
        using var channel = _persistentConnection.CreateModel();
        channel.ExchangeDeclare(exchange: BROKER_NAME, type: "direct");
        var body = JsonSerializer.SerializeToUtf8Bytes(@event, @event.GetType(), new JsonSerializerOptions { WriteIndented = true });

        policy.Execute(() =>
        {
            var properties = channel.CreateBasicProperties();
            properties.DeliveryMode = 2;
            channel.BasicPublish(exchange: BROKER_NAME, routingKey: eventName, mandatory: true, basicProperties: properties, body: body);
        });
        return Task.CompletedTask;
    }

    public void Subscribe<T, TH>() where T : Events.IntegrationEvent where TH : IIntegrationEventHandler<T>
    {
        var eventName = _subsManager.GetEventKey<T>();
        DoInternalSubscription(eventName);
        _logger.LogInformation("Subscribing to event {EventName} with {EventHandler}", eventName, typeof(TH).Name);
        _subsManager.AddSubscription<T, TH>();
        StartBasicConsume();
    }

    private void DoInternalSubscription(string eventName)
    {
        if (_subsManager.HasSubscriptionsForEvent(eventName)) return;
        if (!_persistentConnection.IsConnected) _persistentConnection.TryConnect();
        _consumerChannel!.QueueBind(queue: _queueName, exchange: BROKER_NAME, routingKey: eventName);
    }

    public void Unsubscribe<T, TH>() where T : Events.IntegrationEvent where TH : IIntegrationEventHandler<T>
    {
        var eventName = _subsManager.GetEventKey<T>();
        _logger.LogInformation("Unsubscribing from event {EventName}", eventName);
        _subsManager.RemoveSubscription<T, TH>();
    }

    public void Dispose()
    {
        _consumerChannel?.Dispose();
        _subsManager.Clear();
    }

    private IModel CreateConsumerChannel()
    {
        if (!_persistentConnection.IsConnected) _persistentConnection.TryConnect();
        var channel = _persistentConnection.CreateModel();
        channel.ExchangeDeclare(exchange: BROKER_NAME, type: "direct");
        channel.QueueDeclare(queue: _queueName, durable: true, exclusive: false, autoDelete: false, arguments: null);
        channel.CallbackException += (sender, ea) =>
        {
            _logger.LogWarning(ea.Exception, "Recreating consumer channel");
            _consumerChannel!.Dispose();
            _consumerChannel = CreateConsumerChannel();
            StartBasicConsume();
        };
        return channel;
    }

    private void StartBasicConsume()
    {
        if (_consumerChannel == null) { _logger.LogError("Cannot start consumer"); return; }
        var consumer = new AsyncEventingBasicConsumer(_consumerChannel);
        consumer.Received += Consumer_Received;
        _consumerChannel.BasicConsume(queue: _queueName, autoAck: false, consumer: consumer);
    }

    private async Task Consumer_Received(object sender, BasicDeliverEventArgs eventArgs)
    {
        var eventName = eventArgs.RoutingKey;
        var message = Encoding.UTF8.GetString(eventArgs.Body.Span);
        try { await ProcessEvent(eventName, message); }
        catch (Exception ex) { _logger.LogWarning(ex, "Error processing message"); }
        _consumerChannel!.BasicAck(eventArgs.DeliveryTag, multiple: false);
    }

    private async Task ProcessEvent(string eventName, string message)
    {
        if (!_subsManager.HasSubscriptionsForEvent(eventName)) { _logger.LogWarning("No subscription for {EventName}", eventName); return; }
        var subscriptions = _subsManager.GetHandlersForEvent(eventName);
        foreach (var subscription in subscriptions)
        {
            var handler = _serviceProvider.GetService(subscription.HandlerType);
            if (handler == null) continue;
            var eventType = _subsManager.GetEventTypeByName(eventName);
            if (eventType == null) continue;
            var integrationEvent = JsonSerializer.Deserialize(message, eventType, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            if (integrationEvent == null) continue;
            var concreteType = typeof(IIntegrationEventHandler<>).MakeGenericType(eventType);
            var method = concreteType.GetMethod("Handle");
            if (method == null) continue;
            await (Task)method.Invoke(handler, new object[] { integrationEvent })!;
        }
    }
}