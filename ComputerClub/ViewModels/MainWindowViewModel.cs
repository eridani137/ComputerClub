using System.Collections.ObjectModel;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ComputerClub.Views;
using Wpf.Ui;
using Wpf.Ui.Abstractions;
using Wpf.Ui.Controls;

namespace ComputerClub.ViewModels;

public partial class MainWindowViewModel(
    INavigationService navigationService,
    INavigationViewPageProvider navigationViewPageProvider,
    ISnackbarService snackbarService,
    IContentDialogService dialogService
) : ObservableObject
{
    public ObservableCollection<NavigationViewItem> MenuItems { get; } = [];

    [RelayCommand]
    private void WindowLoaded(FluentWindow window)
    {
        if (window is not MainWindow mainWindow) return;

        mainWindow.NavigationView.SetPageProviderService(navigationViewPageProvider);
        navigationService.SetNavigationControl(mainWindow.NavigationView);

        mainWindow.NavigationView.Navigated += NavigationViewOnNavigated;

        dialogService.SetDialogHost(mainWindow.ContentDialog);
        snackbarService.SetSnackbarPresenter(mainWindow.SnackbarPresenter);
    }

    private void NavigationViewOnNavigated(NavigationView sender, NavigatedEventArgs args)
    {
    }

    [RelayCommand]
    private void Close()
    {
        Application.Current.Shutdown();
    }
}