using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Indexer;

public class StartupService : IHostedService
{
    private readonly ILogger _logger;


    public StartupService(ILogger<StartupService> logger)
    {
        _logger = logger;

    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        _logger.Log(LogLevel.Information, "Application started");

        
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}