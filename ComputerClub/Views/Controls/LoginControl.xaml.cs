using System.Windows;
using System.Windows.Controls;
using ComputerClub.ViewModels;

namespace ComputerClub.Views.Controls;

public partial class LoginControl : UserControl
{
    public LoginControl()
    {
        InitializeComponent();
    }
    
    private void PasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
    {
        if (DataContext is LoginWindowViewModel vm && sender is PasswordBox pb)
        {
            vm.ValidatePassword(pb.Password);
        }
    }
}