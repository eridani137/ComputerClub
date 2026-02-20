using System.Collections.ObjectModel;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ComputerClub.Views;
using Microsoft.AspNetCore.Identity;
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
    ILogger<MainWindowViewModel> logger
) : ObservableObject
{
    public ObservableCollection<NavigationViewItem> MenuItems { get; } = [];

    [RelayCommand]
    private void Loaded(FluentWindow window)
    {
        if (window is not MainWindow mainWindow) return;

        if (App.CurrentUser is not { } user || App.CurrentRole is not { } role) return;

        logger.LogInformation("LoggedOn: [{Role}] {Username}", role, user.UserName);

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
                        "Управление ПК",
                        SymbolRegular.Grid24,
                        typeof(ManagementView)));

                homePageType = typeof(ManagementView);

                break;
            default:
                return;
        }

        navigationService.Navigate(homePageType);
    }

    [RelayCommand]
    private void Close()
    {
        Application.Current.Shutdown();
    }
}