using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using ComputerClub.Models;
using Microsoft.Xaml.Behaviors;

namespace ComputerClub.Handlers;

public class DragBehavior : Behavior<FrameworkElement>
{
    private Point _mouseOffset;
    private CanvasItem? _dataContext;

    protected override void OnAttached()
    {
        base.OnAttached();
        AssociatedObject.MouseLeftButtonDown += OnMouseDown;
        AssociatedObject.MouseMove += OnMouseMove;
        AssociatedObject.MouseLeftButtonUp += OnMouseUp;
    }

    protected override void OnDetaching()
    {
        base.OnDetaching();
        AssociatedObject.MouseLeftButtonDown -= OnMouseDown;
        AssociatedObject.MouseMove -= OnMouseMove;
        AssociatedObject.MouseLeftButtonUp -= OnMouseUp;
    }

    private void OnMouseDown(object sender, MouseButtonEventArgs e)
    {
        if (AssociatedObject.DataContext is not CanvasItem item) return;
        _dataContext = item;

        var canvas = Extensions.FindParent<Canvas>(AssociatedObject);
        if (canvas == null) return;

        var elementPos = AssociatedObject.TranslatePoint(new Point(0, 0), canvas);
        var mousePos = e.GetPosition(canvas);
        _mouseOffset = new Point(mousePos.X - elementPos.X, mousePos.Y - elementPos.Y);

        AssociatedObject.CaptureMouse();
        e.Handled = true;
    }

    private void OnMouseMove(object sender, MouseEventArgs e)
    {
        if (_dataContext == null || e.LeftButton != MouseButtonState.Pressed) return;

        var canvas = Extensions.FindParent<Canvas>(AssociatedObject);
        if (canvas == null) return;

        var pos = e.GetPosition(canvas);
        var newX = pos.X - _mouseOffset.X;
        var newY = pos.Y - _mouseOffset.Y;

        // var maxX = canvas.ActualWidth - AssociatedObject.ActualWidth;
        // var maxY = canvas.ActualHeight - AssociatedObject.ActualHeight;
        // if (newX < 0) newX = 0;
        // if (newY < 0) newY = 0;
        // if (newX > maxX) newX = maxX;
        // if (newY > maxY) newY = maxY;

        _dataContext.X = newX;
        _dataContext.Y = newY;
    }

    private void OnMouseUp(object sender, MouseButtonEventArgs e)
    {
        AssociatedObject.ReleaseMouseCapture();
        _dataContext = null;
    }
}