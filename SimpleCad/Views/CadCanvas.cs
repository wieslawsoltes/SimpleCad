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
        Drawing.CurrentTool?.OnPointerPressed(sender, e);
    }

    private void OnPointerReleased(object? sender, PointerReleasedEventArgs e)
    {
        Drawing.CurrentTool?.OnPointerReleased(sender, e);
    }

    private void OnPointerMoved(object? sender, PointerEventArgs e)
    {
        Drawing.CurrentTool?.OnPointerMoved(sender, e);
    }
    
    private void OnPointerWheelChanged(object? sender, PointerWheelEventArgs e)
    {
        Drawing.CurrentTool?.OnPointerMoved(sender, e);
    }

    public override void Render(DrawingContext context)
    {
        base.Render(context);
        
        Drawing.Render(context, new Rect(0, 0, Bounds.Width, Bounds.Height));
    }

    public void Invalidate()
    {
        InvalidateVisual();
    }
}
