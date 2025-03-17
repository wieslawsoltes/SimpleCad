using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;
using SimpleCad.ViewModels;

namespace SimpleCad.Views;

public partial class CadCanvas : Control, ICanvasService
{
    public CadCanvas()
    {
        Drawing = new DrawingViewModel(this);

        PointerPressed += OnPointerPressed;
        PointerReleased += OnPointerReleased;
        PointerMoved += OnPointerMoved;
        PointerWheelChanged += OnPointerWheelChanged;
    }

    public DrawingViewModel Drawing { get; set; }

    private void OnPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        Drawing.OnPointerPressed(sender, e);
    }

    private void OnPointerReleased(object? sender, PointerReleasedEventArgs e)
    {
        Drawing.OnPointerReleased(sender, e);
    }

    private void OnPointerMoved(object? sender, PointerEventArgs e)
    {
        Drawing.OnPointerMoved(sender, e);
    }
    
    private void OnPointerWheelChanged(object? sender, PointerWheelEventArgs e)
    {
        Drawing.OnPointerWheelChanged(sender, e);
    }

    public override void Render(DrawingContext context)
    {
        base.Render(context);
        
        context.FillRectangle(Brushes.Transparent, Bounds);

        context.Custom(new CustomDrawOperation(Bounds, Drawing));
    }

    public double GetHeight()
    {
        return Bounds.Height;
    }

    public void Invalidate()
    {
        InvalidateVisual();
    }
}
