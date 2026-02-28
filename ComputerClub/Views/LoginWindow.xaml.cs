using ComputerClub.ViewModels;
using Wpf.Ui.Controls;

namespace ComputerClub.Views;

public partial class LoginWindow : FluentWindow
{
    public LoginWindow(LoginWindowViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }
}