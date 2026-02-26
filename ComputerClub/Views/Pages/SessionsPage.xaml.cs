using System.Windows.Controls;
using ComputerClub.ViewModels.Pages;

namespace ComputerClub.Views.Pages;

public partial class SessionsPage : Page
{
    public SessionsPage(SessionsViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }
}