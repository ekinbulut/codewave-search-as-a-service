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
        
        return _publisher.PublishAsync(cancellationToken);
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}