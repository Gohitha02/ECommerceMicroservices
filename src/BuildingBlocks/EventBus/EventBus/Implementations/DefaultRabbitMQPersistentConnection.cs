using System.Net.Sockets;
using Microsoft.Extensions.Logging;
using Polly;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using RabbitMQ.Client.Exceptions;

namespace EventBus.Implementations;

public class DefaultRabbitMQPersistentConnection : IRabbitMQPersistentConnection
{
    private readonly IConnectionFactory _connectionFactory;
    private readonly ILogger<DefaultRabbitMQPersistentConnection> _logger;
    private readonly int _retryCount;
    private IConnection? _connection;
    private bool _disposed;
    readonly object _syncRoot = new();

    public DefaultRabbitMQPersistentConnection(IConnectionFactory connectionFactory, ILogger<DefaultRabbitMQPersistentConnection> logger, int retryCount = 5)
    {
        _connectionFactory = connectionFactory ?? throw new ArgumentNullException(nameof(connectionFactory));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _retryCount = retryCount;
    }

    public bool IsConnected => _connection is { IsOpen: true } && !_disposed;

    public IModel CreateModel()
    {
        if (!IsConnected)
            throw new InvalidOperationException("No RabbitMQ connections are available");
        return _connection!.CreateModel();
    }

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;
        try { _connection?.Dispose(); }
        catch (IOException ex) { _logger.LogCritical(ex.ToString()); }
    }

    public bool TryConnect()
    {
        _logger.LogInformation("RabbitMQ Client is trying to connect");
        lock (_syncRoot)
        {
            var policy = Policy.Handle<SocketException>()
                .Or<BrokerUnreachableException>()
                .WaitAndRetry(_retryCount, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),
                    (ex, time) => _logger.LogWarning(ex, "Could not connect after {TimeOut}s", $"{time.TotalSeconds:n1}"));

            policy.Execute(() => _connection = _connectionFactory.CreateConnection());

            if (!IsConnected) return false;

            _connection!.ConnectionShutdown += OnConnectionShutdown;
            _connection.CallbackException += OnCallbackException;
            _connection.ConnectionBlocked += OnConnectionBlocked;
            _logger.LogInformation("Connected to '{HostName}'", _connection.Endpoint.HostName);
            return true;
        }
    }

    private void OnConnectionBlocked(object? sender, ConnectionBlockedEventArgs e) => TryConnect();
    private void OnCallbackException(object? sender, CallbackExceptionEventArgs e) => TryConnect();
    private void OnConnectionShutdown(object? sender, ShutdownEventArgs reason) => TryConnect();
}