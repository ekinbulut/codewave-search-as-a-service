namespace Contracts;

public class MessageContract
{
    public ulong DeliveryTag { get; set; }
    public object Datas { get; set; }

    public DateTime Published => DateTime.UtcNow;
}