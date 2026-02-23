using System.Windows.Controls;
using ManagementViewModel = ComputerClub.ViewModels.Pages.ManagementViewModel;

namespace ComputerClub.Views.Pages;

public partial class Management : UserControl
{
    public Management(ManagementViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }
}