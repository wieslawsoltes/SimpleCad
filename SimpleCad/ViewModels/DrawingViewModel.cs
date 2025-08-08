using System.IO;
using System.Linq;
using Avalonia;
using Avalonia.Input;
using DynamicData;
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
        CurrentTool = new LineTool(this, canvasService, PanAndZoomService);
        _dxfFile = DxfFile.Create();
    }

    public ICanvasService CanvasService { get; }
    
    public PanAndZoomService PanAndZoomService { get; }

    public DxfWriterService DxfWriterService { get; }

    public DxfReaderService DxfReaderService { get; }

    public Tool? CurrentTool { get; set; }
    
    public event System.Action<DxfFile>? FileOpened;

    private DxfFile _dxfFile;
    
    public DxfFile DxfFile 
    { 
        get => _dxfFile;
        private set => SetProperty(ref _dxfFile, value);
    }

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
        DxfFile.AddEntity(dxfEntity);
    }
    
    public void Remove(DxfEntity dxfEntity)
    {
        DxfFile.RemoveEntity(dxfEntity);
    }
    
    public void Render(SKCanvas context, Rect bounds)
    {
        var paint = new SKPaint
        {
            Color = SKColor.Parse("#212830"), 
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

        DxfFile.Render(context, bounds, transform.ScaleX);
        
        // Render selection highlights
        RenderSelectionHighlights(context, transform.ScaleX);
        
        context.Restore();
    }

    private void RenderSelectionHighlights(SKCanvas context, double zoomFactor)
    {
        if (CurrentTool is SelectionTool selectionTool && selectionTool.SelectedEntities.Count > 0)
        {
            using var selectionPaint = new SKPaint
            {
                Color = SKColors.Cyan,
                Style = SKPaintStyle.Stroke,
                StrokeWidth = (float)(2.0 / zoomFactor),
                IsAntialias = true
            };

            using var dashEffect = SKPathEffect.CreateDash(new float[] { 5.0f / (float)zoomFactor, 5.0f / (float)zoomFactor }, 0);
            selectionPaint.PathEffect = dashEffect;

            foreach (var entity in selectionTool.SelectedEntities)
            {
                var bounds = entity.GetBounds();
                context.DrawRect(bounds, selectionPaint);
            }
        }

        // Render selection rectangle during drag
        if (CurrentTool is SelectionTool dragSelectionTool && dragSelectionTool.IsDragging)
        {
            using var dragPaint = new SKPaint
            {
                Color = SKColors.Blue.WithAlpha(128),
                Style = SKPaintStyle.Stroke,
                StrokeWidth = (float)(1.0 / zoomFactor),
                IsAntialias = true
            };

            var dragRect = dragSelectionTool.GetDragRectangle();
            if (dragRect.HasValue)
            {
                context.DrawRect(dragRect.Value, dragPaint);
            }
        }
    }

    public void Open(Stream stream)
    {
        using var reader = new StreamReader(stream);

        var dxfFile = DxfReaderService.ReadDxfFile(reader);

        dxfFile.UpdateObject();
        
        // TODO: Invalidate entities children
        var entities = dxfFile.GetEntities();
        foreach (var entity in entities)
        {
            entity.Invalidate();
        }

        DxfFile = dxfFile;
        
        // Notify that the file has been opened
        FileOpened?.Invoke(dxfFile);

        CanvasService.Invalidate();
    }

    public void SaveAs(Stream stream)
    {
        using var writer = new StreamWriter(stream);

        DxfWriterService.WriteDxfFile(writer, DxfFile);
    }
}
