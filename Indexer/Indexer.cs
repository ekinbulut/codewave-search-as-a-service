using System.Text;
using Broker;
using Contracts;
using DatabaseAdaptor;
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
        var data = JsonConvert.DeserializeObject<MessageContract>(JsonConvert.SerializeObject(response.Data));
        _rabbitMqBroker.Ack(data.DeliveryTag);
        //TODO: retry
    }

    private void RabbitMqBrokerOnMessageReceived(byte[] data, ulong deliveryTag)
    {
        _logger.Log(LogLevel.Information, $"Message received");
        
        var dataAsString = Encoding.UTF8.GetString(data);
        var messageContract = JsonConvert.DeserializeObject<MessageContract>(dataAsString);
        messageContract.DeliveryTag = deliveryTag;

        try
        {

            var databaseModel = JsonConvert.DeserializeObject<DatabaseModel>(JsonConvert.SerializeObject(messageContract.Data));
            var task = _elasticSearchAdaptor.IndexAsync(databaseModel, "product_index");
            task.Wait(CancellationToken.None);

        }
        catch (Exception error)
        {
            _logger.LogError(error.ToString(), error);
        }

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