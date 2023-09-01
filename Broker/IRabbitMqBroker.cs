namespace Broker;

public interface IRabbitMqBroker
{
    void Send(string exchange, string queue, string routingKey, string message);
    Task SendAsync(string exchange, string queue, string routingKey, string message, CancellationToken cancellationToken = new CancellationToken());
    void Consume(string queue);
    Task ConsumeAsync(string queue, CancellationToken cancellationToken);
    void CancelConsumer();
    void CloseConnections();
}