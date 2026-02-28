using System.Collections.ObjectModel;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ComputerClub.Views;
using ComputerClub.Views.Pages;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Wpf.Ui;
using Wpf.Ui.Abstractions;
using Wpf.Ui.Controls;

namespace ComputerClub.ViewModels;

public partial class MainWindowViewModel(
    INavigationService navigationService,
    INavigationViewPageProvider navigationViewPageProvider,
    ISnackbarService snackbarService,
    IContentDialogService dialogService,
    ILogger<MainWindowViewModel> logger,
    IServiceScopeFactory scopeFactory
) : ObservableObject
{
    private FluentWindow _window;
    
    public ObservableCollection<NavigationViewItem> MenuItems { get; } = [];

    [RelayCommand]
    private void Loaded(FluentWindow window)
    {
        _window = window;
        
        if (window is not MainWindow mainWindow) return;

        if (App.CurrentUser is not { } user || App.CurrentRole is not { } role) return;

        logger.LogInformation("Авторизация: [{Role}] {Username}", role, user.UserName);

        mainWindow.NavigationView.SetPageProviderService(navigationViewPageProvider);
        navigationService.SetNavigationControl(mainWindow.NavigationView);

        dialogService.SetDialogHost(mainWindow.ContentDialog);
        snackbarService.SetSnackbarPresenter(mainWindow.SnackbarPresenter);

        Type? homePageType;

        switch (role)
        {
            case "Admin":
                MenuItems.Add(
                    new NavigationViewItem(
                        "Управление компьютерами",
                        SymbolRegular.Grid24,
                        typeof(ComputersManagementPage)));

                MenuItems.Add(
                    new NavigationViewItem(
                        "Управление клиентами",
                        SymbolRegular.Person24,
                        typeof(ClientsPage)));
                
                MenuItems.Add(new NavigationViewItem(
                    "Тарифы",
                    SymbolRegular.Notebook24,
                    typeof(TariffsPage)));
                
                MenuItems.Add(new NavigationViewItem(
                    "Сессии",
                    SymbolRegular.NotepadPerson24,
                    typeof(SessionsPage)));

                MenuItems.Add(new NavigationViewItem()
                {
                    Content = "Отчеты",
                    Icon = new SymbolIcon() { Symbol = SymbolRegular.DocumentBulletList24 },
                    IsExpanded = true,
                    MenuItems =
                    {
                        new NavigationViewItem(
                            "Текущая касса",
                            SymbolRegular.Money24,
                            typeof(CurrentCashPage)),

                        new NavigationViewItem(
                            "Текущий отчет",
                            SymbolRegular.ClipboardBulletListRtl20,
                            typeof(CurrentReportPage))
                    }
                });

                MenuItems.Add(new NavigationViewItem(
                    "Development",
                    SymbolRegular.Code24,
                    typeof(DevPage)));

                homePageType = typeof(ComputersManagementPage);

                break;
            default:
                return;
        }

        navigationService.Navigate(homePageType);
    }

    [RelayCommand]
    private void Logout()
    {
        var scope = scopeFactory.CreateScope();
        App.ShowLoginWindow(scope.ServiceProvider);
        _window.Close();
    }
    
    [RelayCommand]
    private static void Close()
    {
        Application.Current.Shutdown();
    }
}