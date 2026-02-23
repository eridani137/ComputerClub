using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;

namespace ComputerClub.Behaviors;

public static class SmartSubmenuBehavior
{
    public static readonly DependencyProperty EnabledProperty =
        DependencyProperty.RegisterAttached(
            "Enabled", typeof(bool), typeof(SmartSubmenuBehavior),
            new PropertyMetadata(false, OnEnabledChanged));

    public static void SetEnabled(DependencyObject obj, bool value) =>
        obj.SetValue(EnabledProperty, value);

    public static bool GetEnabled(DependencyObject obj) =>
        (bool)obj.GetValue(EnabledProperty);

    private static void OnEnabledChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not MenuItem menuItem || !(bool)e.NewValue) return;

        menuItem.SubmenuOpened += (_, _) =>
        {
            if (menuItem.Template?.FindName("Popup", menuItem) is not Popup popup) return;

            popup.Placement = PlacementMode.Custom;
            popup.CustomPopupPlacementCallback = (popupSize, targetSize, _) =>
            {
                var workArea = SystemParameters.WorkArea;
                var origin = menuItem.PointToScreen(new Point(0, 0));

                var x = targetSize.Width;
                double y = 0;

                if (origin.X + targetSize.Width + popupSize.Width > workArea.Right)
                {
                    x = -popupSize.Width;
                }

                if (origin.Y + popupSize.Height > workArea.Bottom)
                {
                    var overflow = (origin.Y + popupSize.Height) - workArea.Bottom;
                    y = -overflow;
                }

                var popupPlacement = new CustomPopupPlacement(new Point(x, y), PopupPrimaryAxis.Horizontal);
                
                return [popupPlacement];
            };
        };
    }
}