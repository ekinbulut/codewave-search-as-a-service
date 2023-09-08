using Broker;
using Contracts;
using DatabaseAdaptor;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Diagnostics.Contracts;

namespace Publisher;

public class Publisher : IPublisher
{
    private readonly IAdaptor _adaptor;
    private readonly IRabbitMqBroker _rabbitMqBroker;
    private readonly ILogger _logger;
    private readonly IOptions<PublisherOptions> _options;

    public Publisher(IAdaptor adaptor, IRabbitMqBroker rabbitMqBroker, ILogger<Publisher> logger, IOptions<PublisherOptions> options)
    {
        _adaptor = adaptor;
        _rabbitMqBroker = rabbitMqBroker;
        _logger = logger;
        _options = options;


        _rabbitMqBroker.MessageSent += RabbitMqBrokerOnMessageSent;

    }

    private void RabbitMqBrokerOnMessageSent(Guid id)
    {
        _logger.Log(LogLevel.Information, $"Message sent: {id}");
        _logger.Log(LogLevel.Information, "Data published");
        
    }

    public async Task PublishAsync(CancellationToken token)
    {
        try
        {
            if (_adaptor.Connect())
            {
                _logger.Log(LogLevel.Information, "Adaptor connected");
                var contracts = GetMessageContracts();
                await Publish(contracts, token);
            }
        }
        catch (Exception e)
        {
            _logger.Log(LogLevel.Error, e, "Error Occurred");
        }
    }

    private string SerializeObject(object obj){
        var serializeObject = JsonConvert.SerializeObject(obj);
        return serializeObject;
    }

    private List<DatabaseModel> GetDatabaseModel(){
        var datas = new List<DatabaseModel>();

        var data = _adaptor.GetDatabaseModel();

        var batchSize = _options.Value.BatchSize;

        foreach (var item in data.Tables)
        {
            if (item.Datas.Count > batchSize)
            {
                var counter = item.Datas.Count;
                var skip = 0;

                while (counter > 0)
                {
                    var newData = new DatabaseModel();
                    newData.Tables = new List<Table>();
                    var batch = item.Datas.Skip(skip).Take(batchSize).ToList();
                    newData.Tables.Add(new Table { Datas = batch, TableName = item.TableName });
                    counter -= batchSize;
                    skip += batchSize;
                    datas.Add(newData);
                }
            }else
            {
                datas.Add(new DatabaseModel { Tables = new List<Table> { new Table { Datas = item.Datas, TableName = item.TableName } } });
            }
        }

        _logger.Log(LogLevel.Information, "Data fetched");
        return datas;
    }

    private List<MessageContract> GetMessageContracts(){
        var data = GetDatabaseModel();
        var messages = new List<MessageContract>();
        foreach (var item in data)
        {
            var message = new MessageContract
            {
                Id = Guid.NewGuid(),
                Data = item
            };
            messages.Add(message);
        }
        
        return messages;
    }

    private async Task Publish(ICollection<MessageContract> messages, CancellationToken cancellationToken)
    {
        var messageCount = messages.Count;
        int index = 0;
        var exchange = _options.Value.Exchange;
        var quoue = _options.Value.Queue;

        while (messageCount > 0)
        {
            var serializeObject = SerializeObject(messages.ToList()[index]); // not optimized
            await _rabbitMqBroker.SendAsync(exchange, quoue, "", serializeObject, cancellationToken);
            messageCount--;
            index++;
        }

        _rabbitMqBroker.CloseConnections();
        _logger.Log(LogLevel.Information, "Connection closed");
    }
}