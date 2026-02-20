using Microsoft.Extensions.Hosting;
using Serilog;
using Serilog.Core;
using Serilog.Events;

namespace ComputerClub.Configuration;

public static class ConfigureLogging
{
    public static void Configure(IHostBuilder builder, LogEventLevel logEventLevel = LogEventLevel.Information)
    {
        const string outputTemplate =
            "[{Timestamp:HH:mm:ss} {Level:u3}] [{SourceContext}] {Message:lj}{NewLine}{Exception}";

        var levelSwitch = new LoggingLevelSwitch(logEventLevel);

        var configuration = new LoggerConfiguration()
            .MinimumLevel.ControlledBy(levelSwitch)
            .Enrich.FromLogContext()
            .WriteTo.Console(outputTemplate: outputTemplate, levelSwitch: levelSwitch);
        
        Log.Logger = configuration.CreateLogger();

        builder.UseSerilog();
    }
}