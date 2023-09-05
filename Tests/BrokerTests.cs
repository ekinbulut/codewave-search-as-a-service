using System.Text;
using Broker;

namespace Tests;

public class BrokerTests
{
    private RabbitMqBroker _sut;

    public BrokerTests()
    {
        _sut = new RabbitMqBroker("guest", "guest", "/", "localhost", 5672);
    }


    [Fact]
    public void Test_Send_Message()
    {
        for (int i = 0; i < 3; i++)
        {
            _sut.Send("test_exchange", "test_queue", "", "{\\data:[]\\}");
        }

        _sut.CloseConnections();
    }

    [Fact]
    public async Task Test_Send_Message_Async()
    {
        for (int i = 0; i < 3; i++)
        {
            await _sut.SendAsync("test_exchange_async", "test_queue_async", "", "{\\data:[]\\}");
        }
        _sut.CloseConnections();
    }

    [Fact]
    public void Test_Consume()
    {
        _sut.Consume("test_queue");
        _sut.MessageReceived += delegate(byte[] bytes, ulong deliveryTag)
        {
            var body = Encoding.UTF8.GetString(bytes);
            Assert.False(String.IsNullOrEmpty(body));
            _sut.Ack(deliveryTag);
        };
        _sut.CancelConsumer();
    }
    
    [Fact]
    public async Task Test_Consume_Async()
    {
        CancellationTokenSource cts = new CancellationTokenSource();
        _sut.MessageReceived += delegate(byte[] bytes, ulong deliveryTag)
        {
            var body = Encoding.UTF8.GetString(bytes);
            Assert.False(String.IsNullOrEmpty(body));
            _sut.Ack(deliveryTag);
           
        };
        await _sut.ConsumeAsync("test_queue_async", cts.Token);
        _sut.CancelConsumer();
    }
}