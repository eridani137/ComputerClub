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
    
    private void PasswordBox_PasswordChanged(object sender, System.Windows.RoutedEventArgs e)
    {
        if (DataContext is LoginWindowViewModel viewModel && sender is PasswordBox passwordBox)
        {
            viewModel.ValidatePassword(passwordBox.Password);
        }
    }
}