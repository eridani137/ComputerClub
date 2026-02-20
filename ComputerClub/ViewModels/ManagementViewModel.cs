using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Wpf.Ui.Controls;

namespace ComputerClub.ViewModels;

public partial class ManagementViewModel : ObservableObject
{
    [RelayCommand]
    private void Loaded(FluentWindow window)
    {

    }
}