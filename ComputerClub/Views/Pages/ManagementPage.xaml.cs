using System.Windows.Controls;
using ManagementViewModel = ComputerClub.ViewModels.Pages.ManagementViewModel;

namespace ComputerClub.Views.Pages;

public partial class ManagementPage : UserControl
{
    public ManagementPage(ManagementViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }
}