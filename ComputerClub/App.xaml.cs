using System.Windows;
using ComputerClub.Configuration;
using ComputerClub.ViewModels;
using ComputerClub.Views;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Wpf.Ui;
using Wpf.Ui.DependencyInjection;

namespace ComputerClub;

public partial class App : Application
{
    private readonly IHost _host;
    private readonly ILogger<App> _logger = new LoggerFactory().CreateLogger<App>();

    public App()
    {
        var builder = Host.CreateDefaultBuilder();

        ConfigureLogging.Configure(builder);

        builder.ConfigureServices((context, services) => ConfigureServices(context.Configuration, services));

        _host = builder.Build();
    }

    protected override async void OnStartup(StartupEventArgs se)
    {
        try
        {
            base.OnStartup(se);

            await _host.StartAsync();
            
            var mainWindow = _host.Services.GetRequiredService<MainWindow>();
            mainWindow.Show();
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Необработанное исключение");
        }
    }

    protected override async void OnExit(ExitEventArgs ee)
    {
        try
        {
            using (_host)
            {
                await _host.StopAsync();
            }

            base.OnExit(ee);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Необработанное исключение");
        }
    }

    private void ConfigureServices(IConfiguration configuration, IServiceCollection services)
    {
        services.AddSingleton<MainWindow>();
        services.AddSingleton<MainWindowViewModel>();
        
        services.AddNavigationViewPageProvider();

        services.AddSingleton<ISnackbarService, SnackbarService>();
        services.AddSingleton<IContentDialogService, ContentDialogService>();
        services.AddSingleton<INavigationService, NavigationService>();
    }
}