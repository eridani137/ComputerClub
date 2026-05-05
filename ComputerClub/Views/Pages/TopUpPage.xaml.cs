using System.Windows.Controls;
using ComputerClub.ViewModels.Pages;

namespace ComputerClub.Views.Pages;

public partial class TopUpPage : Page
{
    public TopUpPage(TopUpViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }
}
