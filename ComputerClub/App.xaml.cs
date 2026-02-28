using System.IO;
using System.Windows;
using ComputerClub.Configuration;
using ComputerClub.Infrastructure;
using ComputerClub.Infrastructure.Entities;
using ComputerClub.Services;
using ComputerClub.ViewModels;
using ComputerClub.ViewModels.Pages;
using ComputerClub.Views;
using ComputerClub.Views.Pages;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;
using Wpf.Ui;
using Wpf.Ui.DependencyInjection;

namespace ComputerClub;

public partial class App : Application
{
    private readonly IHost _host;

    public static ComputerClubIdentity? CurrentUser { get; set; }
    public static string? CurrentRole { get; set; }

    public App()
    {
        var builder = Host.CreateDefaultBuilder();

        ConfigureLogging.Configure(builder);

        builder.ConfigureAppConfiguration((_, configuration) =>
        {
            configuration.SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false);
        });

        builder.ConfigureServices((context, services) => ConfigureServices(context.Configuration, services));

        _host = builder.Build();
    }

    protected override async void OnStartup(StartupEventArgs se)
    {
        try
        {
            base.OnStartup(se);

            await _host.StartAsync();

            await using var scope = _host.Services.CreateAsyncScope();
            
            var seeder = scope.ServiceProvider.GetRequiredService<DatabaseSeeder>();
            await seeder.Seed();
            var tickService = scope.ServiceProvider.GetRequiredService<SessionTickService>();
            tickService.Start();

            ShowLoginWindow(_host.Services);
        }
        catch (Exception e)
        {
            Log.Error(e, "Необработанное исключение");
        }
    }
    
    public void ShowLoginWindow(IServiceProvider services)
    {
        CurrentUser = null;
        CurrentRole = null;
        
        var loginWindow = services.GetRequiredService<LoginWindow>();
        Current.MainWindow = loginWindow;
        loginWindow.Closed += (_, _) =>
        {
            if (Current.MainWindow is not ComputerClub.Views.MainWindow)
            {
                Current.Shutdown();
            }
        };
        loginWindow.Show();
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
            Log.Error(e, "Необработанное исключение");
        }
    }

    private static void ConfigureServices(IConfiguration configuration, IServiceCollection services)
    {
        services.AddScoped<LoginWindow>();
        services.AddScoped<LoginWindowViewModel>();

        services.AddTransient<MainWindow>();
        services.AddTransient<MainWindowViewModel>();

        services.AddTransient<ComputersManagementViewModel>();
        services.AddTransient<CurrentCashViewModel>();
        services.AddTransient<CurrentReportViewModel>();
        services.AddTransient<ClientsViewModel>();
        services.AddTransient<TariffsViewModel>();
        services.AddTransient<SessionsViewModel>();
        services.AddTransient<DevViewModel>();

        services.AddTransient<ComputersManagementPage>();
        services.AddTransient<CurrentCashPage>();
        services.AddTransient<CurrentReportPage>();
        services.AddTransient<ClientsPage>();
        services.AddTransient<TariffsPage>();
        services.AddTransient<SessionsPage>();
        services.AddTransient<DevPage>();

        services.AddNavigationViewPageProvider();

        services.AddSingleton<ISnackbarService, SnackbarService>();
        services.AddSingleton<IContentDialogService, ContentDialogService>();
        services.AddSingleton<INavigationService, NavigationService>();

        services.AddTransient<SessionService>();
        services.AddSingleton<SessionTickService>();

        AddInfrastructure(configuration, services);
    }

    private static void AddInfrastructure(IConfiguration configuration, IServiceCollection services)
    {
        services.AddScoped<DatabaseSeeder>();

        services.AddDbContext<ApplicationDbContext>(options =>
        {
            options.UseSqlServer(configuration.GetConnectionString("DefaultConnection"));
        });

        services.AddIdentityCore<ComputerClubIdentity>()
            .AddRoles<IdentityRole<int>>()
            .AddEntityFrameworkStores<ApplicationDbContext>();
    }
}