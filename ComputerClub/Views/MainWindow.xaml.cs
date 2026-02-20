using ComputerClub.ViewModels;
using Wpf.Ui.Controls;

namespace ComputerClub.Views;

public partial class MainWindow : FluentWindow
{
    public MainWindow(MainWindowViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }
}