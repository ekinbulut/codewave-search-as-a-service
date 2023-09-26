using System.Text;
using Broker;
using Contracts;
using DatabaseAdaptor;
using ElasticsearchAdaptor;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
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
    private readonly IOptions<IndexerOptions> _options;

    public Indexer(ILogger<Indexer> logger, IRabbitMqBroker rabbitMqBroker, IElasticSearchAdaptor elasticSearchAdaptor, IOptions<IndexerOptions> options)
    {
        _logger = logger;
        _rabbitMqBroker = rabbitMqBroker;
        _elasticSearchAdaptor = elasticSearchAdaptor;
        _options = options;

        RegisterEventListeners();
    }

    private void RegisterEventListeners()
    {
        _rabbitMqBroker.MessageReceived += MessageReceivedEvent;
        _elasticSearchAdaptor.IndexResponse += IndexResponseEvent;
    }

    private void IndexResponseEvent(object arg1, ElasticSearchResponse response)
    {
        _logger.Log(LogLevel.Information, $"Status: {response.Status} with code {response.Code}");
        var data = JsonConvert.DeserializeObject<DatabaseModel>(JsonConvert.SerializeObject(response.Data));
        _rabbitMqBroker.Ack(data.DeliveryTag);
    }

    private void MessageReceivedEvent(byte[] data, ulong deliveryTag)
    {
        _logger.Log(LogLevel.Information, $"Message received");
        
        var dataAsString = Encoding.UTF8.GetString(data);
        var messageContract = JsonConvert.DeserializeObject<MessageContract>(dataAsString);
        if (messageContract != null) Index(messageContract, deliveryTag);
    }

    private void Index(MessageContract messageContract, ulong deliveryTag)
    {
        try
        {
            var databaseModel = JsonConvert.DeserializeObject<DatabaseModel>(JsonConvert.SerializeObject(messageContract.Data));
            if (databaseModel == null) return;
            databaseModel.DeliveryTag = deliveryTag;
            var task = _elasticSearchAdaptor.IndexAsync(databaseModel, _options.Value.Index);
            task.Wait(CancellationToken.None);
        }
        catch (Exception error)
        {
            _logger.Log(LogLevel.Error, error, error.ToString());
        }
    }

    public Task ConsumeAsync(CancellationToken token)
    {
        return _rabbitMqBroker.ConsumeAsync(_options.Value.Queue, token);
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