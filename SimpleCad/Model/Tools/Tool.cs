using Avalonia.Input;

namespace SimpleCad.Model;

public abstract class Tool
{
    public Tool(IDrawingService drawingService, ICanvasService canvasService)
    {
        DrawingService = drawingService;
        CanvasService = canvasService;
    }

    public IDrawingService DrawingService { get; }
    
    public ICanvasService CanvasService { get; }

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
}
