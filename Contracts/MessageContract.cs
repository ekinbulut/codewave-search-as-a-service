namespace Contracts;

public class MessageContract
{
    public Guid Id { get; set; }
    public ulong DeliveryTag { get; set; }
    public object Data { get; set; }
    public DateTime Published => DateTime.UtcNow;
}