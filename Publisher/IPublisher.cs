namespace Publisher;

public interface IPublisher
{
    Task PublishAsync(CancellationToken token);
}