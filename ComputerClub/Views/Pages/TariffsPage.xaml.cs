using System.Windows.Controls;
using ComputerClub.ViewModels.Pages;

namespace ComputerClub.Views.Pages;

public partial class TariffsPage : Page
{
    public TariffsPage(TariffsViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }
}