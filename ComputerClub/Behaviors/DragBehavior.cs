using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using ComputerClub.Models;
using Microsoft.Xaml.Behaviors;

namespace ComputerClub.Behaviors;

public class DragBehavior : Behavior<FrameworkElement>
{
    private Point _mouseOffset;
    private CanvasItem? _dataContext;
    private Canvas? _canvas;
    public double GridSizeX { get; set; } = 1;
    public double GridSizeY { get; set; } = 1;

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

        var elementWidth = AssociatedObject.ActualWidth > 0
            ? AssociatedObject.ActualWidth
            : AssociatedObject.Width;

        var elementHeight = AssociatedObject.ActualHeight > 0
            ? AssociatedObject.ActualHeight
            : AssociatedObject.Height;

        var maxX = _canvas.ActualWidth - elementWidth;
        var maxY = _canvas.ActualHeight - elementHeight;

        var rawX = Math.Max(0, Math.Min(pos.X - _mouseOffset.X, maxX));
        var rawY = Math.Max(0, Math.Min(pos.Y - _mouseOffset.Y, maxY));

        var newX = Math.Round(rawX / GridSizeX) * GridSizeX;
        var newY = Math.Round(rawY / GridSizeY) * GridSizeY;

        if (IsColliding(newX, newY, elementWidth, elementHeight)) return;

        _dataContext.X = newX;
        _dataContext.Y = newY;
    }

    private void OnMouseUp(object sender, MouseButtonEventArgs e)
    {
        AssociatedObject.ReleaseMouseCapture();
        _dataContext = null;
        _canvas = null;
    }
    
    private bool IsColliding(double newX, double newY, double width, double height)
    {
        if (_canvas == null) return false;

        var itemsControl = Extensions.FindParent<ItemsControl>(_canvas);
        if (itemsControl?.ItemsSource is not IEnumerable<CanvasItem> items) return false;

        var newRight = newX + width;
        var newBottom = newY + height;

        foreach (var item in items)
        {
            if (item == _dataContext) continue;

            var otherRight = item.X + width;
            var otherBottom = item.Y + height;

            var overlap =
                newX < otherRight &&
                newRight > item.X &&
                newY < otherBottom &&
                newBottom > item.Y;

            if (overlap) return true;
        }

        return false;
    }
}