using Broker;
using DatabaseAdaptor;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

namespace Publisher;

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
        _builder.Services.AddSingleton<IRabbitMqBroker, RabbitMqBroker>();

        var dbOptions = _builder.Services.BuildServiceProvider().GetRequiredService<IOptions<DatabaseOptions>>();
        Console.WriteLine($"Database: {dbOptions.Value.ConnectionString?.Split('=')[1]}");

        var pbOptions = _builder.Services.BuildServiceProvider().GetRequiredService<IOptions<PublisherOptions>>();
        Console.WriteLine($"Exchange: {pbOptions.Value.Exchange}");
        Console.WriteLine($"Queue: {pbOptions.Value.Queue}");
        Console.WriteLine($"Batch: {pbOptions.Value.BatchSize}");



        _builder.Services.AddTransient<IAdaptor, SqlLiteAdaptor>(p => new SqlLiteAdaptor(dbOptions.Value.ConnectionString));
        _builder.Services.AddTransient<IPublisher, Publisher>();
        _builder.Services.AddHostedService<StartupService>();
    }
    public void RegisterConfigs()
    {
        IHostEnvironment env = _builder.Environment;

        _builder.Configuration
            .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
            .AddJsonFile($"appsettings.{env.EnvironmentName}.json", true, true);

        _builder.Services.Configure<RabbitMqOptions>(_builder.Configuration.GetSection(nameof(RabbitMqOptions)));
        _builder.Services.Configure<DatabaseOptions>(_builder.Configuration.GetSection(nameof(DatabaseOptions)));
        _builder.Services.Configure<PublisherOptions>(_builder.Configuration.GetSection(nameof(PublisherOptions)));


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