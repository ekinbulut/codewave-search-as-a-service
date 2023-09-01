using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Publisher;

public class StartupService : IHostedService
{
    private readonly ILogger _logger;
    private IPublisher _publisher;

    public StartupService(ILogger<StartupService> logger, IPublisher publisher)
    {
        _logger = logger;
        _publisher = publisher;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        _logger.Log(LogLevel.Information, "Application started");

        var task = _publisher.PublishAsync(cancellationToken);
        Task.WaitAll(task);
        
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}