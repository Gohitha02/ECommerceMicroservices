#nullable disable
using RabbitMQ.Client;

namespace EventBus.Implementations;

public interface IRabbitMQPersistentConnection : IDisposable
{
    bool IsConnected { get; }
    bool TryConnect();
    IModel CreateModel();
}