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
        var expected = 3;

        _sut.Consume("test_queue_async");

        while (Messages.Queue.Count != expected)
        {
            if (Messages.Queue.Count == expected)
            {
                break;
            }
        }

        Assert.Equal(expected, Messages.Queue.Count);
        _sut.CancelConsumer();
    }
    
    [Fact]
    public async Task Test_Consume_Async()
    {
        var expected = 3;
        CancellationTokenSource cts = new CancellationTokenSource();
        await _sut.ConsumeAsync("test_queue", cts.Token);

        while (Messages.Queue.Count != expected)
        {
            if (Messages.Queue.Count == expected)
            {
                break;
            }
        }

        Assert.Equal(expected, Messages.Queue.Count);
        _sut.CancelConsumer();
    }
}