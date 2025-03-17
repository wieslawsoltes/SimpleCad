using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using Avalonia;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.Platform;
using Avalonia.Rendering.SceneGraph;
using Avalonia.Skia;
using ReactiveUI;
using SkiaSharp;

namespace SimpleCad.ViewModels;

public class ViewModelBase : ReactiveObject
{
}

public abstract class Tool : ViewModelBase
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

public class LineTool : Tool
{
    private DxfLineEntity? _currentEntity;
    
    public LineTool(IDrawingService drawingService, ICanvasService canvasService)
        : base(drawingService, canvasService)
    {
    }

    public override void OnPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        base.OnPointerPressed(sender, e);

        var position = e.GetPosition(sender as Visual);

        _currentEntity = Add(position);

        CanvasService.Invalidate();
    }

    public override void OnPointerReleased(object? sender, PointerReleasedEventArgs e)
    {
        base.OnPointerReleased(sender, e);
        
        if (_currentEntity is null)
        {
            return;
        }
    
        Move(_currentEntity, sender, e);

        CanvasService.Invalidate();

        _currentEntity = null;
    }

    public override void OnPointerMoved(object? sender, PointerEventArgs e)
    {
        base.OnPointerMoved(sender, e);

        if (_currentEntity is null)
        {
            return;
        }

        Move(_currentEntity, sender, e);

        CanvasService.Invalidate();
    }

    private DxfLineEntity Add(Point position)
    {
        var (x, y) = position;
        var height = CanvasService.GetHeight();
        y = height - y;

        var entity = new DxfLineEntity
        {
            StartPointX = x,
            StartPointY = y,
            EndPointX = x,
            EndPointY = y,
        };
        
        entity.Invalidate();

        DrawingService.Add(entity);

        return entity;
    }

    private void Move(DxfLineEntity entity, object? sender, PointerEventArgs e)
    {
        var position = e.GetPosition(sender as Visual);

        var (x, y) = position;
        var height = CanvasService.GetHeight();
        y = height - y;
        
        if (e.KeyModifiers == KeyModifiers.Shift)
        {
            var deltaX = Math.Abs(x - entity.StartPointX);
            var deltaY = Math.Abs(y - entity.StartPointY);

            if (deltaX > deltaY)
            {
                entity.EndPointX = x;
                entity.EndPointY = entity.StartPointY;
            }
            else
            {
                entity.EndPointX = entity.StartPointX;
                entity.EndPointY = y;
            }
        }
        else
        {
            entity.EndPointX = x;
            entity.EndPointY = y;
        }
        
        entity.Invalidate();
    }
}

public abstract class DxfEntity : ViewModelBase
{
    public abstract void Render(SKCanvas context, double zoomFactor);

    public abstract void Invalidate();

    public abstract bool Contains(float x, float y);
}

public class DxfLineEntity : DxfEntity
{
    private SKPaint _pen;
    private double _thickness = 1.0;
    private SKPath? _path;
    private SKPath? _fillPath;

    public DxfLineEntity()
    {
        _pen = new SKPaint
        {
            Style = SKPaintStyle.Stroke,
            Color = SKColors.White,
            StrokeWidth = (float)_thickness,
        };
    }

    public double StartPointX { get; set; }
    
    public double StartPointY { get; set; }
    
    public double EndPointX { get; set; }
    
    public double EndPointY { get; set; }

    public override void Render(SKCanvas context, double zoomFactor)
    {
        if (_path is null)
        {
            return;
        }

        _pen.StrokeWidth = (float)(_thickness / zoomFactor);

        context.DrawPath(_path, _pen);
    }

    public override void Invalidate()
    {
        _path = CreatePath();
        _fillPath = _pen.GetFillPath(_path);
    }

    public override bool Contains(float x, float y)
    {
        if (_fillPath is null)
        {
            return false;
        }

        return _fillPath.Contains(x, y);
    }

    private SKPath CreatePath()
    {
        var path = new SKPath();

        path.MoveTo((float)StartPointX, (float)StartPointY);
        path.LineTo((float)EndPointX, (float)EndPointY);

        return path;
    }
}

public class DxfDrawing : ViewModelBase
{
    public DxfDrawing()
    {
        Entities = new List<DxfEntity>();
    }

    public List<DxfEntity> Entities { get; set; }

    public void Invalidate()
    {
        foreach (var entity in Entities)
        {
            entity.Invalidate();
        }
    }

    public void Render(SKCanvas context, Rect bounds, double zoomFactor)
    {
        context.Save();

        context.Translate((float)0.0, (float)bounds.Height);
        context.Scale((float)1.0, (float)-1.0);

        foreach (var entity in Entities)
        {
            entity.Render(context, zoomFactor);
        }
        
        context.Restore();
    }
}

public interface IDrawingService
{
    void Add(DxfEntity dxfEntity);
}

public interface ICanvasService
{
    double GetHeight();
    
    void Invalidate();
}

public class DxfWriterService
{
    public void WriteDxfDrawing(StreamWriter writer, DxfDrawing dxfDrawing)
    {
        WriteHeaderSection(writer);
        WriteEntitiesSection(writer, dxfDrawing.Entities);
        WriteEofSection(writer);
    }
    
    private void WriteHeaderSection(StreamWriter writer)
    {
        writer.WriteLine("0");
        writer.WriteLine("SECTION");
        writer.WriteLine("2");
        writer.WriteLine("HEADER");

        // TODO: Variables

        writer.WriteLine("0");
        writer.WriteLine("ENDSEC");
    }
    
    private void WriteEntitiesSection(StreamWriter writer, List<DxfEntity> entities)
    {
        writer.WriteLine("0");
        writer.WriteLine("SECTION");
        writer.WriteLine("2");
        writer.WriteLine("ENTITIES");

        foreach (var entity in entities)
        {
            switch (entity)
            {
                case DxfLineEntity lineEntity:
                    WriteLineEntity(writer, lineEntity);
                    break;
            }
        }

        writer.WriteLine("0");
        writer.WriteLine("ENDSEC");
    }

    private void WriteLineEntity(StreamWriter writer, DxfLineEntity dxfLineEntity)
    {
        writer.WriteLine("0");
        writer.WriteLine("LINE");

        writer.WriteLine("10");
        writer.WriteLine(dxfLineEntity.StartPointX.ToString(CultureInfo.InvariantCulture));

        writer.WriteLine("20");
        writer.WriteLine(dxfLineEntity.StartPointY.ToString(CultureInfo.InvariantCulture));

        writer.WriteLine("11");
        writer.WriteLine(dxfLineEntity.EndPointX.ToString(CultureInfo.InvariantCulture));

        writer.WriteLine("21");
        writer.WriteLine(dxfLineEntity.EndPointY.ToString(CultureInfo.InvariantCulture));
    }

    private void WriteEofSection(StreamWriter writer)
    {
        writer.WriteLine("0");
        writer.WriteLine("EOF");
    }
}

public class DxfReaderService
{
    private void ReadDxfLineEntity(DxfLineEntity dxfLineEntity, int code, string data)
    {
        if (code == 10)
        {
            dxfLineEntity.StartPointX = double.Parse(data.Trim(), CultureInfo.InvariantCulture);
        }
        else if (code == 20)
        {
            dxfLineEntity.StartPointY = double.Parse(data.Trim(), CultureInfo.InvariantCulture);
        }
        else if (code == 11)
        {
            dxfLineEntity.EndPointX = double.Parse(data.Trim(), CultureInfo.InvariantCulture);   
        }
        else if (code == 21)
        {
            dxfLineEntity.EndPointY = double.Parse(data.Trim(), CultureInfo.InvariantCulture); 
        }
    }

    public DxfDrawing ReadDxfDrawing(StreamReader reader)
    {
        var dxfDrawing = new DxfDrawing();

        object? currentObject = null;

        while (true)
        {
            var codeLine = reader.ReadLine();
            var dataLine = reader.ReadLine();

            if (string.IsNullOrEmpty(codeLine) || dataLine is null)
            {
                break;
            }

            var code = int.Parse(codeLine.Trim(), CultureInfo.InvariantCulture);

            if (code == 0)
            {
                currentObject = null;

                if (dataLine == "EOF")
                {
                }
                else if (dataLine == "SECTION")
                {
                    
                }
                else if (dataLine == "ENDSEC")
                {
                    
                }
                else if (dataLine == "LINE")
                {
                    var lineEntity = new DxfLineEntity();
                    dxfDrawing.Entities.Add(lineEntity);
                    currentObject = lineEntity;
                }
            }
            else
            {
                if (currentObject is DxfLineEntity lineEntity)
                {
                    ReadDxfLineEntity(lineEntity, code, dataLine);
                }
            }
        } 

        return dxfDrawing;
    }
}

public class DrawingViewModel : ViewModelBase, IDrawingService
{
    private const double _baseZoomFactor = 1.15;
    private const int _minZoomLevel = -20;
    private const int _maxZoomLevel = 40;
    private int _currentZoomLevel;
    private double _zoomFactor = 1.0;
    private SKMatrix _transform = SKMatrix.Identity;
    private bool _isPanning;
    private Point _lastPanPosition;

    public DrawingViewModel(ICanvasService canvasService)
    {
        CanvasService = canvasService;
        DxfWriterService = new DxfWriterService();
        DxfReaderService = new DxfReaderService();
        CurrentTool = new LineTool(this, canvasService);
        DxfDrawing = new DxfDrawing();
    }

    public ICanvasService CanvasService { get; }

    public DxfWriterService DxfWriterService { get; }

    public DxfReaderService DxfReaderService { get; }

    public Tool? CurrentTool { get; set; }

    public DxfDrawing DxfDrawing { get; private set; }

    public void OnPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (TryStartPan(sender, e))
        {
            return;
        }

        CurrentTool?.OnPointerPressed(sender, e);
    }

    public void OnPointerReleased(object? sender, PointerReleasedEventArgs e)
    {
        if (TryEndPan(e))
        {
            return;
        }

        CurrentTool?.OnPointerReleased(sender, e);
    }

    public void OnPointerMoved(object? sender, PointerEventArgs e)
    {
        if (TryMovePan(sender, e))
        {
            CanvasService.Invalidate();

            return;
        }

        CurrentTool?.OnPointerMoved(sender, e);
    }

    public void OnPointerWheelChanged(object? sender, PointerWheelEventArgs e)
    {
        Zoom(sender, e);

        CurrentTool?.OnPointerWheelChanged(sender, e);
    }

    private bool TryStartPan(object? sender, PointerPressedEventArgs e)
    {
        if (!e.GetCurrentPoint(null).Properties.IsMiddleButtonPressed)
        {
            return false;
        }

        _isPanning = true;
        _lastPanPosition = e.GetPosition(sender as Visual);
        e.Handled = true;

        return true;
    }

    private bool TryEndPan(PointerReleasedEventArgs e)
    {
        if (!_isPanning)
        {
            return false;
        }
        
        _isPanning = false;
        e.Handled = true;

        return true;
    }

    private bool TryMovePan(object? sender, PointerEventArgs e)
    {
        if (!_isPanning)
        {
            return false;
        }

        var position = e.GetPosition(sender as Visual);
        var delta = position - _lastPanPosition;

        _transform = SKMatrix.Concat(SKMatrix.CreateTranslation((float)delta.X, (float)delta.Y), _transform);
  
        _lastPanPosition = position;

        e.Handled = true;

        return true;
    }

    private void Zoom(object? sender, PointerWheelEventArgs e)
    {
        var position = e.GetPosition(sender as Visual);
        var zoomDelta = e.Delta.Y > 0 ? 1 : -1;

        var newZoomLevel = Math.Clamp(_currentZoomLevel + zoomDelta, _minZoomLevel, _maxZoomLevel);
        if (newZoomLevel == _currentZoomLevel)
        {
            return;
        }

        var oldZoomFactor = _zoomFactor;
        _currentZoomLevel = newZoomLevel;
        _zoomFactor = Math.Pow(_baseZoomFactor, _currentZoomLevel);

        var scaleFactor = _zoomFactor / oldZoomFactor;

        _transform = SKMatrix.Concat(SKMatrix.CreateTranslation((float)-position.X, (float)-position.Y), _transform);
        _transform = SKMatrix.Concat(SKMatrix.CreateScale((float)scaleFactor, (float)scaleFactor), _transform);
        _transform = SKMatrix.Concat(SKMatrix.CreateTranslation((float)position.X, (float)position.Y), _transform);

        CanvasService.Invalidate();
    }

    public void Add(DxfEntity dxfEntity)
    {
        DxfDrawing.Entities.Add(dxfEntity);
    }
    
    public void Render(SKCanvas context, Rect bounds)
    {
        var paint = new SKPaint
        {
            Color = SKColors.Black, 
            Style = SKPaintStyle.Fill
        };

        context.Save();

        context.DrawRect(new SKRect(
            (float)bounds.X, 
            (float)bounds.Y, 
            (float)bounds.Width, 
            (float)bounds.Height), 
            paint);

        context.Translate(_transform.TransX, _transform.TransY);
        context.Scale(_transform.ScaleX, _transform.ScaleY);

        DxfDrawing.Render(context, bounds, _transform.ScaleX);
        
        context.Restore();
    }

    public void Open(Stream stream)
    {
        using var reader = new StreamReader(stream);

        var dxfDrawing = DxfReaderService.ReadDxfDrawing(reader);

        dxfDrawing.Invalidate();
        
        DxfDrawing = dxfDrawing;

        CanvasService.Invalidate();
    }

    public void SaveAs(Stream stream)
    {
        using var writer = new StreamWriter(stream);

        DxfWriterService.WriteDxfDrawing(writer, DxfDrawing);
    }
}

public class CustomDrawOperation : ICustomDrawOperation
{
    private readonly Rect _bounds;
    private readonly DrawingViewModel _drawingViewModel;

    public CustomDrawOperation(Rect bounds, DrawingViewModel drawingViewModel)
    {
        _bounds = bounds;
        _drawingViewModel = drawingViewModel;
    }

    public Rect Bounds => _bounds;

    public bool Equals(ICustomDrawOperation? other)
    {
        return false;
    }

    public void Dispose()
    {
    }

    public bool HitTest(Point p)
    {
        return false;
    }

    public void Render(ImmediateDrawingContext context)
    {
        var skiaSharpApiLeaseFeature = context.TryGetFeature<ISkiaSharpApiLeaseFeature>();
        if (skiaSharpApiLeaseFeature is null)
        {
            return;
        }
        
        using var skiaSharpApiLease = skiaSharpApiLeaseFeature.Lease();

        _drawingViewModel.Render(
            skiaSharpApiLease.SkCanvas, 
            new Rect(0, 0, _bounds.Width, _bounds.Height));
    }
}
