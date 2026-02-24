using System.Windows;
using System.Windows.Media;

namespace ComputerClub.Extensions;

public static class DependencyExtensions
{
    public static T? FindParent<T>(DependencyObject child) where T : DependencyObject
    {
        var parent = VisualTreeHelper.GetParent(child);

        while (parent is not null && parent is not T)
        {
            parent = VisualTreeHelper.GetParent(parent);
        }

        return parent as T;
    }
}