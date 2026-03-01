using System.Windows.Controls;
using ComputerClub.ViewModels.Pages;

namespace ComputerClub.Views.Pages;

public partial class CurrentCashPage : Page
{
    public CurrentCashPage(CurrentCashViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }
}