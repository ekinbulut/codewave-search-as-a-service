using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace Broker;

public class RabbitMqBroker : IRabbitMqBroker
{
    private readonly string _username;
    private readonly string _password;
    private readonly string _virtualHost;
    private readonly string _hostname;
    private readonly int _port;

    private readonly ConnectionFactory _factory;
    private IConnection _connection;
    private IModel _channel;
    
    private string _consumerTag;
    
    public RabbitMqBroker(string username, string password, string virtualHost, string hostname, int port)
    {
        this._username = username;
        this._password = password;
        this._virtualHost = virtualHost;
        this._hostname = hostname;
        this._port = port;

        _factory = new ConnectionFactory();
    }

    private IConnection Connection
    {
        get
        {
            _factory.UserName = _username;
            _factory.Password = _password;
            _factory.VirtualHost = _virtualHost;
            _factory.HostName = _hostname;
            _factory.Port = _port;
            _connection = _factory.CreateConnection();
            return _connection;
        }
    }

    private IModel Channel
    {
        get
        {
            if (_channel == null)
            {
                _channel = Connection.CreateModel();
            }

            return _channel;
        }
    }

    public void Send(string exchange, string queue, string routingKey, string message)
    {
        byte[] messageBodyBytes = System.Text.Encoding.UTF8.GetBytes(message);
        
        Channel.ExchangeDeclare(exchange, ExchangeType.Direct);
        Channel.QueueDeclare(queue, true, false, false, null);
        Channel.QueueBind(queue, exchange, routingKey, null);
        
        Channel.BasicPublish(exchange,routingKey, null,messageBodyBytes);
        
    }
    
    public async Task SendAsync(string exchange, string queue, string routingKey, string message, CancellationToken cancellationToken = new CancellationToken())
    {
        var task = Task.Factory.StartNew(() =>
        {
            byte[] messageBodyBytes = System.Text.Encoding.UTF8.GetBytes(message);
        
            Channel.ExchangeDeclare(exchange, ExchangeType.Direct);
            Channel.QueueDeclare(queue, true, false, false, null);
            Channel.QueueBind(queue, exchange, routingKey, null);
        
            Channel.BasicPublish(exchange,routingKey, null,messageBodyBytes);

        }, cancellationToken);
        
        await task;
    }

    public void Consume(string queue)
    {
        var consumer = new EventingBasicConsumer(Channel);
        consumer.Received += (ch, ea) =>
        {
            var body = ea.Body.ToArray();
            Messages.Queue.Enqueue(body);
            Channel.BasicAck(ea.DeliveryTag, false);
        };

        _consumerTag = Channel.BasicConsume(queue, false, consumer);
    }

    public Task ConsumeAsync(string queue, CancellationToken cancellationToken)
    {
        _factory.DispatchConsumersAsync = true;

        var consumer = new AsyncEventingBasicConsumer(Channel);
        consumer.Received += async (ch, ea) =>
        {
            var body = ea.Body.ToArray();
            Messages.Queue.Enqueue(body);
            Channel.BasicAck(ea.DeliveryTag, false);
            await Task.Yield();
        };
        _consumerTag = Channel.BasicConsume(queue, false, consumer);
        return Task.CompletedTask;
    }

    public void CancelConsumer()
    {
        _channel.BasicCancel(_consumerTag);
    }

    public void CloseConnections()
    {
        Channel.Close();
        Connection.Close();
    }
}