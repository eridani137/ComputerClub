using System.Windows.Controls;
using ComputerClub.ViewModels.Pages;

namespace ComputerClub.Views.Pages;

public partial class PaymentsPage : Page
{
    public PaymentsPage(PaymentsViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }
}