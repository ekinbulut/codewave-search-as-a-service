using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

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
        _builder.Services.AddHostedService<StartupService>();
    }
    public void RegisterConfigs()
    {
        IHostEnvironment env = _builder.Environment;

        _builder.Configuration
            .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
            .AddJsonFile($"appsettings.{env.EnvironmentName}.json", true, true);
    
    
        RabbitMqOptions rabbitMqOptions = new RabbitMqOptions();
        _builder.Configuration.GetSection(nameof(RabbitMqOptions)).Bind(rabbitMqOptions);

        Console.WriteLine(rabbitMqOptions.Hostname);
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