using System.Windows.Controls;
using ComputerClub.ViewModels.Pages;

namespace ComputerClub.Views.Pages;

public partial class DevPage : Page
{
    public DevPage(DevViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }
}