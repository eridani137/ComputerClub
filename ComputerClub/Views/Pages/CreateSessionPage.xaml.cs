using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using ComputerClub.Models;
using ComputerClub.ViewModels.Pages;

namespace ComputerClub.Views.Pages;

public partial class CreateSessionPage : Page
{
    public CreateSessionPage(CreateSessionViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
        
        Loaded += (_, _) =>
        {
            var parentScroll = FindParentScrollViewer(this);
            parentScroll?.VerticalScrollBarVisibility = ScrollBarVisibility.Disabled;
        };
    }
    
    private static ScrollViewer? FindParentScrollViewer(DependencyObject child)
    {
        var parent = VisualTreeHelper.GetParent(child);
        while (parent is not null)
        {
            if (parent is ScrollViewer sv) return sv;
            parent = VisualTreeHelper.GetParent(parent);
        }
        return null;
    }
    
    private ScheduleRow? GetRow(FrameworkElement fe)
    {
        DependencyObject? current = fe;
        while (current is not null)
        {
            if (current is FrameworkElement { DataContext: ScheduleRow row })
            {
                return row;
            }
            current = VisualTreeHelper.GetParent(current);
        }
        return null;
    }

    private void Cell_Click(object sender, MouseButtonEventArgs e)
    {
        if (sender is not FrameworkElement fe) return;
        if (fe.Tag is not ScheduleCell cell) return;
        var row = GetRow(fe);
        if (row is null) return;
        ((CreateSessionViewModel)DataContext).OnCellClick(row, cell);
    }
}