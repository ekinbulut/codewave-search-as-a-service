using Broker;
using DatabaseAdaptor;
using Microsoft.Extensions.Logging;

namespace Publisher;

public class Publisher : IPublisher
{
    private readonly IAdaptor _adaptor;
    private readonly IRabbitMqBroker _rabbitMqBroker;
    private readonly ILogger _logger;

    public Publisher(IAdaptor adaptor, IRabbitMqBroker rabbitMqBroker, ILogger<Publisher> logger)
    {
        _adaptor = adaptor;
        _rabbitMqBroker = rabbitMqBroker;
        _logger = logger;
    }

    public async Task PublishAsync(CancellationToken token)
    {
        try
        {
            if (_adaptor.Connect())
            {
                _logger.Log(LogLevel.Information, "Adaptor connected");
                var data = _adaptor.GetSchemaAndData();
                _logger.Log(LogLevel.Information, "Data fetched");
                await _rabbitMqBroker.SendAsync("publisher_exchange", "publisher_queue", "", data, token);
                _logger.Log(LogLevel.Information, "Data published");
                _rabbitMqBroker.CloseConnections();
                _logger.Log(LogLevel.Information, "Connection closed");
            }
        }
        catch (Exception e)
        {
            _logger.Log(LogLevel.Error, e, "Error Occurred");
        }
    }
}