using System.Windows;
using System.Windows.Controls;
using ComputerClub.ViewModels;

namespace ComputerClub.Views.Controls;

public partial class RegisterControl : UserControl
{
    public RegisterControl()
    {
        InitializeComponent();
    }
    
    private void PasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
    {
        if (DataContext is LoginWindowViewModel vm && sender is PasswordBox pb)
        {
            vm.ValidateRegPassword(pb.Password);
        }
    }
}