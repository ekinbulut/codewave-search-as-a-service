using Broker;
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
        Console.WriteLine($"MQ: ampq://{mqOptions.Value.Hostname}:{mqOptions.Value.Port}");
        _builder.Services.AddSingleton<IRabbitMqBroker>(p => new RabbitMqBroker(mqOptions.Value.Username,mqOptions.Value.Password, mqOptions.Value.VirtualHost, mqOptions.Value.Hostname, mqOptions.Value.Port));


        
        _builder.Services.AddHostedService<StartupService>();
    }
    public void RegisterConfigs()
    {
        IHostEnvironment env = _builder.Environment;

        _builder.Configuration
            .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
            .AddJsonFile($"appsettings.{env.EnvironmentName}.json", true, true);

        _builder.Services.Configure<RabbitMqOptions>(_builder.Configuration.GetSection(nameof(RabbitMqOptions)));

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