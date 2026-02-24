using System.IO;
using System.Windows;
using ComputerClub.Configuration;
using ComputerClub.Infrastructure;
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
using Wpf.Ui;
using Wpf.Ui.DependencyInjection;
using ManagementViewModel = ComputerClub.ViewModels.Pages.ManagementViewModel;

namespace ComputerClub;

public partial class App : Application
{
    private readonly IHost _host;
    private readonly ILogger<App> _logger = new LoggerFactory().CreateLogger<App>();
    
    public static IdentityUser? CurrentUser { get; set; }
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

            var loginWindow = _host.Services.GetRequiredService<LoginWindow>();
            loginWindow.Show();
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

    private static void ConfigureServices(IConfiguration configuration, IServiceCollection services)
    {
        services.AddScoped<LoginWindow>();
        services.AddScoped<LoginWindowViewModel>();
        
        services.AddScoped<MainWindow>();
        services.AddScoped<MainWindowViewModel>();
        
        services.AddScoped<ManagementViewModel>();
        services.AddScoped<CurrentCashViewModel>();
        services.AddScoped<CurrentReportViewModel>();
        services.AddScoped<ClientsViewModel>();
        services.AddScoped<TariffsViewModel>();
        services.AddScoped<SessionsViewModel>();
        
        services.AddScoped<ComputersManagementPage>();
        services.AddScoped<CurrentCashPage>();
        services.AddScoped<CurrentReportPage>();
        services.AddScoped<ClientsPage>();

        services.AddNavigationViewPageProvider();

        services.AddSingleton<ISnackbarService, SnackbarService>();
        services.AddSingleton<IContentDialogService, ContentDialogService>();
        services.AddSingleton<INavigationService, NavigationService>();
        
        services.AddScoped<SessionService>();

        AddInfrastructure(configuration, services);
    }

    private static void AddInfrastructure(IConfiguration configuration, IServiceCollection services)
    {
        services.AddScoped<DatabaseSeeder>();
        
        services.AddDbContext<ApplicationDbContext>(options =>
        {
            options.UseNpgsql(configuration.GetConnectionString("Database"));
        });

        services.AddIdentityCore<IdentityUser>(options =>
            {
                options.Password.RequireDigit = true;
                options.Password.RequiredLength = 8;
                options.Password.RequireNonAlphanumeric = true;
                options.Password.RequireUppercase = true;
                options.Password.RequireLowercase = true;
                options.Password.RequiredUniqueChars = 3;
            })
            .AddRoles<IdentityRole>()
            .AddEntityFrameworkStores<ApplicationDbContext>();
    }
}