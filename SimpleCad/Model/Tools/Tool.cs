using Avalonia;
using Avalonia.Input;

namespace SimpleCad.Model;

public abstract class Tool
{
    public Tool(IDrawingService drawingService, ICanvasService canvasService, IPanAndZoomService panAndZoomService)
    {
        DrawingService = drawingService;
        CanvasService = canvasService;
        PanAndZoomService = panAndZoomService;
    }

    public IDrawingService DrawingService { get; }
    
    public ICanvasService CanvasService { get; }
    
    public IPanAndZoomService PanAndZoomService { get; }

    public virtual void OnPointerPressed(object? sender, PointerPressedEventArgs e)
    {
    }

    public virtual void OnPointerReleased(object? sender, PointerReleasedEventArgs e)
    {
    }

    public virtual void OnPointerMoved(object? sender, PointerEventArgs e)
    {
    }

    public virtual void OnPointerWheelChanged(object? sender, PointerWheelEventArgs e)
    {
    }

    protected Point Map(Point position)
    {
        var transform = PanAndZoomService.Transform.Invert();
        var point = transform.MapPoint((float)position.X, (float)position.Y);
        return new Point(point.X, point.Y);
    }
}
