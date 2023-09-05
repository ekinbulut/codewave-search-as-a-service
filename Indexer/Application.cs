using Broker;
using ElasticsearchAdaptor;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

namespace Indexer;

public class Application
{
    private readonly HostApplicationBuilder _builder;
    public Application(string[] args)
    {
        _builder = new HostApplicationBuilder(args);
        _builder.Configuration.Sources.Clear();
    }
    public void RegisterServices()
    {
        var mqOptions = _builder.Services.BuildServiceProvider().GetRequiredService<IOptions<RabbitMqOptions>>();
        var esOptions = _builder.Services.BuildServiceProvider().GetRequiredService<IOptions<ElasticSearchOptions>>();
        Console.WriteLine($"MQ: ampq://{mqOptions.Value.Hostname}:{mqOptions.Value.Port}");
        Console.WriteLine($"Elastic search URL:{esOptions.Value.Uri}");

        _builder.Services.AddSingleton<IRabbitMqBroker, RabbitMqBroker>();
        _builder.Services.AddTransient<IElasticSearchAdaptor, ElasticSearchAdaptor>();

        _builder.Services.AddHostedService<StartupService>();
    }
    public void RegisterConfigs()
    {
        IHostEnvironment env = _builder.Environment;

        _builder.Configuration
            .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
            .AddJsonFile($"appsettings.{env.EnvironmentName}.json", true, true);

        _builder.Services.Configure<RabbitMqOptions>(_builder.Configuration.GetSection(nameof(RabbitMqOptions)));
        _builder.Services.Configure<ElasticSearchOptions>(_builder.Configuration.GetSection(nameof(ElasticSearchOptions)));

    }

    public void DisplayInfo()
    {
        var version = typeof(Program).Assembly.GetName().Version;
        var name = typeof(Program).Assembly.GetName().Name;
        Console.WriteLine($"Name: {name}");
        Console.WriteLine($"Version: {version}");
    }

    public async Task RunAsync()
    {
        using IHost host = _builder.Build();
        await host.RunAsync();
    }
}