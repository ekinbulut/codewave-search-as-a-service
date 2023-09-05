using System.Text;
using Broker;
using ElasticsearchAdaptor;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace Indexer;

public interface IIndexer
{
    Task ConsumeAsync(CancellationToken token);
    Task StopAsync(CancellationToken token);
}

public class Indexer : IIndexer
{
    private readonly ILogger _logger;
    private readonly IRabbitMqBroker _rabbitMqBroker;
    private readonly IElasticSearchAdaptor _elasticSearchAdaptor;

    public Indexer(ILogger<Indexer> logger, IRabbitMqBroker rabbitMqBroker, IElasticSearchAdaptor elasticSearchAdaptor)
    {
        _logger = logger;
        _rabbitMqBroker = rabbitMqBroker;
        _elasticSearchAdaptor = elasticSearchAdaptor;
        
        RegisterEventListeners();
    }

    private void RegisterEventListeners()
    {
        _rabbitMqBroker.MessageReceived += RabbitMqBrokerOnMessageReceived;
        _elasticSearchAdaptor.AdaptorResponse += ElasticSearchAdaptorOnAdaptorResponse;
    }

    private void ElasticSearchAdaptorOnAdaptorResponse(object arg1, ElasticSearchResponse response)
    {
        _logger.Log(LogLevel.Information, $"Status: {response.Status} with code {response.Code}");
        
        //TODO: retry
    }

    private void RabbitMqBrokerOnMessageReceived(byte[] data, ulong deliveryTag)
    {
        _logger.Log(LogLevel.Information, $"Message received");
        
        var dataAsString = Encoding.UTF8.GetString(data);
        var o = JsonConvert.DeserializeObject<dynamic>(dataAsString);

        // TODO: not working you have to try convert to data object
        var task = _elasticSearchAdaptor.IndexAsync(o, "product_index");
        task.Wait(CancellationToken.None);
        
        _rabbitMqBroker.Ack(deliveryTag);
    }

    public Task ConsumeAsync(CancellationToken token)
    {
        return _rabbitMqBroker.ConsumeAsync("publisher_queue", token);
    }

    public Task StopAsync(CancellationToken token)
    {
        _logger.Log(LogLevel.Information, "Stopping Indexer");
        _rabbitMqBroker.CancelConsumer();
        _rabbitMqBroker.CloseConnections();
        _logger.Log(LogLevel.Information, "Bye!");
        return Task.CompletedTask;
    }
}