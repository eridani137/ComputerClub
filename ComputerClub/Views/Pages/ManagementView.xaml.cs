using System.Windows.Controls;
using ManagementViewModel = ComputerClub.ViewModels.Pages.ManagementViewModel;

namespace ComputerClub.Views.Pages;

public partial class ManagementView : UserControl
{
    public ManagementView(ManagementViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }
}