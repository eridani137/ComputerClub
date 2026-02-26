using System.Windows.Controls;
using ComputerClub.ViewModels.Pages;

namespace ComputerClub.Views.Pages;

public partial class ComputersManagementPage : Page
{
    public ComputersManagementPage(ComputersManagementViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }
}