using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Indexer;

public class StartupService : IHostedService
{
    private readonly ILogger _logger;
    private readonly IIndexer _indexer;


    public StartupService(ILogger<StartupService> logger, IIndexer indexer)
    {
        _logger = logger;
        _indexer = indexer;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        _logger.Log(LogLevel.Information, "Application started");
        return _indexer.ConsumeAsync(cancellationToken);
    }


    public Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.Log(LogLevel.Information, "Stopping application...");
        return _indexer.StopAsync(cancellationToken);
    }
    
}