using System.Windows.Controls;
using ComputerClub.ViewModels.Pages;

namespace ComputerClub.Views.Pages;

public partial class ClientsPage : Page
{
    public ClientsPage(ClientsViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }
}