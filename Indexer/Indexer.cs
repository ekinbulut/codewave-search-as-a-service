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
        var data = (MessageContract)response.Data!;
        _rabbitMqBroker.Ack(data.DeliveryTag);
        //TODO: retry
    }

    private void RabbitMqBrokerOnMessageReceived(byte[] data, ulong deliveryTag)
    {
        _logger.Log(LogLevel.Information, $"Message received");
        
        var dataAsString = Encoding.UTF8.GetString(data);
        var messageContract = JsonConvert.DeserializeObject<MessageContract>(dataAsString);
        messageContract.DeliveryTag = deliveryTag;
        // TODO: not working you have to try convert to data object
        
        // This is a PROBLEM !!!!!!!
        var databaseModel = (DatabaseModel)messageContract.Datas;

        // Limit and index
        foreach (var table in databaseModel.Tables)
        {
            if (table.Datas.Count > 100)
            {
                table.Datas = table.Datas.Take(100).Select(x => x).ToList();
            }
        }
        
        var task = _elasticSearchAdaptor.IndexAsync(messageContract, "product_index");
        task.Wait(CancellationToken.None);
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