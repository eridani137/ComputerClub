using System.Windows.Controls;
using ManagementViewModel = ComputerClub.ViewModels.Pages.ManagementViewModel;

namespace ComputerClub.Views.Pages;

public partial class ComputersManagementPage : Page
{
    public ComputersManagementPage(ManagementViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }
}