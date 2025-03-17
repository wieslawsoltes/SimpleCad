using System.IO;
using Avalonia;
using Avalonia.Input;
using SimpleCad.Model;
using SkiaSharp;

namespace SimpleCad.ViewModels;

public class DrawingViewModel : ViewModelBase, IDrawing, IDrawingService
{
    public DrawingViewModel(ICanvasService canvasService)
    {
        CanvasService = canvasService;
        PanAndZoomService = new PanAndZoomService();
        DxfWriterService = new DxfWriterService();
        DxfReaderService = new DxfReaderService();
        CurrentTool = new LineTool(this, canvasService);
        DxfEntities = new DxfEntities();
    }

    public ICanvasService CanvasService { get; }
    
    public PanAndZoomService PanAndZoomService { get; }

    public DxfWriterService DxfWriterService { get; }

    public DxfReaderService DxfReaderService { get; }

    public Tool? CurrentTool { get; set; }

    public DxfEntities DxfEntities { get; private set; }

    public void OnPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (PanAndZoomService.TryStartPan(sender, e))
        {
            return;
        }

        CurrentTool?.OnPointerPressed(sender, e);
    }

    public void OnPointerReleased(object? sender, PointerReleasedEventArgs e)
    {
        if (PanAndZoomService.TryEndPan(e))
        {
            return;
        }

        CurrentTool?.OnPointerReleased(sender, e);
    }

    public void OnPointerMoved(object? sender, PointerEventArgs e)
    {
        if (PanAndZoomService.TryMovePan(sender, e))
        {
            CanvasService.Invalidate();

            return;
        }

        CurrentTool?.OnPointerMoved(sender, e);
    }

    public void OnPointerWheelChanged(object? sender, PointerWheelEventArgs e)
    {
        if (PanAndZoomService.Zoom(sender, e))
        {
            CanvasService.Invalidate();
        }

        CurrentTool?.OnPointerWheelChanged(sender, e);
    }

    public void Add(DxfEntity dxfEntity)
    {
        DxfEntities.Entities.Add(dxfEntity);
    }
    
    public void Render(SKCanvas context, Rect bounds)
    {
        var paint = new SKPaint
        {
            Color = SKColors.Black, 
            Style = SKPaintStyle.Fill
        };

        var transform = PanAndZoomService.Transform;
        
        context.Save();

        context.DrawRect(new SKRect(
                (float)bounds.X, 
                (float)bounds.Y, 
                (float)bounds.Width, 
                (float)bounds.Height), 
            paint);

        context.Translate(transform.TransX, transform.TransY);
        context.Scale(transform.ScaleX, transform.ScaleY);

        DxfEntities.Render(context, bounds, transform.ScaleX);
        
        context.Restore();
    }

    public void Open(Stream stream)
    {
        using var reader = new StreamReader(stream);

        var dxfDrawing = DxfReaderService.ReadDxfDrawing(reader);

        dxfDrawing.Invalidate();
        
        DxfEntities = dxfDrawing;

        CanvasService.Invalidate();
    }

    public void SaveAs(Stream stream)
    {
        using var writer = new StreamWriter(stream);

        DxfWriterService.WriteDxfDrawing(writer, DxfEntities);
    }
}
