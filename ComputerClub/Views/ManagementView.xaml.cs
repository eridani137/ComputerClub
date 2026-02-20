using System.Windows.Controls;
using ComputerClub.ViewModels;

namespace ComputerClub.Views;

public partial class ManagementView : UserControl
{
    public ManagementView(ManagementViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }
}