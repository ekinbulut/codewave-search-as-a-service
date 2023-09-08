using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace Broker;

public class RabbitMqBroker : IRabbitMqBroker
{
    private readonly string? _username;
    private readonly string? _password;
    private readonly string? _virtualHost;
    private readonly string? _hostname;
    private readonly int _port;

    private readonly ConnectionFactory? _factory;
    private IConnection? _connection;
    private IModel? _channel;
    
    private string? _consumerTag;

    private readonly IOptions<RabbitMqOptions>? _options;

    public event Action<byte[], ulong>? MessageReceived;
    public event Action<Guid>? MessageSent;

    public RabbitMqBroker(string? username, string? password, string? virtualHost, string? hostname, int port)
    {
        this._username = username;
        this._password = password;
        this._virtualHost = virtualHost;
        this._hostname = hostname;
        this._port = port;

        _factory = new ConnectionFactory
        {
            AutomaticRecoveryEnabled = true,
            NetworkRecoveryInterval = TimeSpan.FromSeconds(10)

        };
    }

    public RabbitMqBroker(IOptions<RabbitMqOptions>? options)
    {
        _options = options;
        _factory = new ConnectionFactory
        {
            AutomaticRecoveryEnabled = true,
            NetworkRecoveryInterval = TimeSpan.FromSeconds(10)
        };
    }

    private IConnection? Connection
    {
        get
        {
            if (_factory != null)
            {
                _factory.UserName = string.IsNullOrEmpty(_username) ? _options?.Value.Username : _username;
                _factory.Password = string.IsNullOrEmpty(_password) ? _options?.Value.Password : _password;
                _factory.VirtualHost = string.IsNullOrEmpty(_virtualHost) ? _options?.Value.VirtualHost : _virtualHost;
                _factory.HostName = string.IsNullOrEmpty(_hostname) ? _options?.Value.Hostname : _hostname;
                if (_options != null) _factory.Port = _port == 0 ? _options.Value.Port : _port;
                _connection = _factory.CreateConnection();
            }

            return _connection;
        }
    }

    private IModel? Channel
    {
        get
        {
            if (_channel == null)
            {
                _channel = Connection?.CreateModel();
            }

            return _channel;
        }
    }

    public void Send(string exchange, string queue, string routingKey, string message)
    {
        byte[] messageBodyBytes = System.Text.Encoding.UTF8.GetBytes(message);
        
        Channel.ExchangeDeclare(exchange, ExchangeType.Direct, true);
        Channel?.QueueDeclare(queue, true, false, false, null);
        Channel?.QueueBind(queue, exchange, routingKey, null);
        
        Channel.BasicPublish(exchange,routingKey, null,messageBodyBytes);
        MessageSent?.Invoke(Guid.NewGuid());
    }
    
    public async Task SendAsync(string exchange, string queue, string routingKey, string message, CancellationToken cancellationToken = new CancellationToken())
    {
        var task = Task.Factory.StartNew(() =>
        {
            byte[] messageBodyBytes = System.Text.Encoding.UTF8.GetBytes(message);
        
            Channel.ExchangeDeclare(exchange, ExchangeType.Direct, true);
            Channel?.QueueDeclare(queue, true, false, false, null);
            Channel?.QueueBind(queue, exchange, routingKey, null);
        
            Channel.BasicPublish(exchange,routingKey, null,messageBodyBytes);
            MessageSent?.Invoke(Guid.NewGuid());
        }, cancellationToken);
        
        await task;
    }

    public void Consume(string queue)
    {
        var consumer = new EventingBasicConsumer(Channel);
        consumer.Received += (ch, ea) =>
        {
            var body = ea.Body.ToArray();
            MessageReceived?.Invoke(body, ea.DeliveryTag);
        };

        _consumerTag = Channel.BasicConsume(queue, false, consumer);
    }

    public Task ConsumeAsync(string queue, CancellationToken cancellationToken)
    {
        if (_factory != null) _factory.DispatchConsumersAsync = true;

        var consumer = new AsyncEventingBasicConsumer(Channel);
        consumer.Received += async (ch, ea) =>
        {
            var body = ea.Body.ToArray();
            MessageReceived?.Invoke(body, ea.DeliveryTag);
            await Task.Yield();
        };
        _consumerTag = Channel.BasicConsume(queue, false, consumer);
        return Task.CompletedTask;
    }

    public void CancelConsumer()
    {
        Channel?.BasicCancel(_consumerTag);
    }

    public void CloseConnections()
    {
        Channel?.Close();
        Connection?.Close();
    }

    public void Ack(ulong deliveryTag)
    {
        Channel?.BasicAck(deliveryTag, false);
    }
}