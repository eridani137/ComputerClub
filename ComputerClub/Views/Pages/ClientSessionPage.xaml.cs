using System.Windows.Controls;
using ComputerClub.ViewModels.Pages;

namespace ComputerClub.Views.Pages;

public partial class ClientSessionPage : Page
{
    public ClientSessionPage(ClientSessionViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }
}