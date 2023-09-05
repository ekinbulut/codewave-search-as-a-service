using System.Text;
using Broker;
using ElasticsearchAdaptor;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace Indexer;

public class StartupService : IHostedService
{
    private readonly ILogger _logger;
    private readonly IRabbitMqBroker _rabbitMqBroker;
    private readonly IElasticSearchAdaptor _elasticSearchAdaptor;


    public StartupService(ILogger<StartupService> logger, IRabbitMqBroker rabbitMqBroker, IElasticSearchAdaptor elasticSearchAdaptor)
    {
        _logger = logger;
        _rabbitMqBroker = rabbitMqBroker;
        _elasticSearchAdaptor = elasticSearchAdaptor;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        _logger.Log(LogLevel.Information, "Application started");

        var brokerTask = _rabbitMqBroker.ConsumeAsync("publisher_queue", cancellationToken);
        _rabbitMqBroker.MessageReceived += RabbitMqBrokerOnMessageReceived;
        _elasticSearchAdaptor.AdaptorResponse += ElasticSearchAdaptorOnAdaptorResponse;
        
        Task.WaitAll(brokerTask);
        
        
        return Task.CompletedTask;
    }

    private void ElasticSearchAdaptorOnAdaptorResponse(object arg1, ElasticSearchResponse response)
    {
        _logger.Log(LogLevel.Information, $"Status: {response.Status} with code {response.Code}");
    }

    private void RabbitMqBrokerOnMessageReceived(byte[] obj)
    {
        var data = Encoding.UTF8.GetString(obj);
        var o = JsonConvert.DeserializeObject<dynamic>(data);
        // TODO: not working you have to try convert to data object
        var task =_elasticSearchAdaptor.IndexAsync(o, "product_index");
        task.Wait(CancellationToken.None);
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
    
}