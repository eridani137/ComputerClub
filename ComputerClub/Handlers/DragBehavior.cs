using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using ComputerClub.Models;
using Microsoft.Xaml.Behaviors;
using Serilog;

namespace ComputerClub.Handlers;

public class DragBehavior : Behavior<FrameworkElement>
{
    private Point _mouseOffset;
    private CanvasItem? _dataContext;
    private Canvas? _canvas;

    protected override void OnAttached()
    {
        AssociatedObject.MouseLeftButtonDown += OnMouseDown;
        AssociatedObject.MouseMove += OnMouseMove;
        AssociatedObject.MouseLeftButtonUp += OnMouseUp;
        base.OnAttached();
    }

    protected override void OnDetaching()
    {
        AssociatedObject.MouseLeftButtonDown -= OnMouseDown;
        AssociatedObject.MouseMove -= OnMouseMove;
        AssociatedObject.MouseLeftButtonUp -= OnMouseUp;
        base.OnDetaching();
    }

    private void OnMouseDown(object sender, MouseButtonEventArgs e)
    {
        if (AssociatedObject.DataContext is not CanvasItem item) return;
        _dataContext = item;

        var presenter = Extensions.FindParent<ContentPresenter>(AssociatedObject);
        if (presenter == null) return;

        if (VisualTreeHelper.GetParent(presenter) is not Canvas canvas) return;
    
        _canvas = canvas;

        var mousePos = e.GetPosition(canvas);
        _mouseOffset = new Point(mousePos.X - item.X, mousePos.Y - item.Y);

        AssociatedObject.CaptureMouse();
        e.Handled = true;
    }

    private void OnMouseMove(object sender, MouseEventArgs e)
    {
        if (_dataContext == null || _canvas == null || e.LeftButton != MouseButtonState.Pressed) return;

        var pos = e.GetPosition(_canvas);

        var maxX = _canvas.ActualWidth - AssociatedObject.ActualWidth;
        var maxY = _canvas.ActualHeight - AssociatedObject.ActualHeight;

        _dataContext.X = Math.Max(0, Math.Min(pos.X - _mouseOffset.X, maxX));
        _dataContext.Y = Math.Max(0, Math.Min(pos.Y - _mouseOffset.Y, maxY));
    }

    private void OnMouseUp(object sender, MouseButtonEventArgs e)
    {
        AssociatedObject.ReleaseMouseCapture();
        _dataContext = null;
        _canvas = null;
    }
}