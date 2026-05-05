using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using ComputerClub.ViewModels.Pages;

namespace ComputerClub.Views.Pages;

public partial class HelpPage : Page
{
    public HelpPage(HelpViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }

    private void MainScrollViewer_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
    {
        if (!e.Handled)
        {
            e.Handled = true;
            var eventArg = new MouseWheelEventArgs(e.MouseDevice, e.Timestamp, e.Delta)
            {
                RoutedEvent = MouseWheelEvent,
                Source = sender
            };
            var parent = ((Control)sender).Parent as UIElement;
            parent?.RaiseEvent(eventArg);
        }
    }
}