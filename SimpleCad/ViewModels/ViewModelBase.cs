using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using Avalonia;
using Avalonia.Input;
using Avalonia.Media;
using ReactiveUI;

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
    }
}

public abstract class DxfEntity : ViewModelBase
{
    public abstract void Render(DrawingContext context);
}

public class DxfLineEntity : DxfEntity
{
    private Pen _pen;

    public DxfLineEntity()
    {
        _pen = new Pen(Brushes.White, 1);
    }

    public double StartPointX { get; set; }
    
    public double StartPointY { get; set; }
    
    public double EndPointX { get; set; }
    
    public double EndPointY { get; set; }

    public override void Render(DrawingContext context)
    {
        context.DrawLine(
            _pen, 
            new Point(StartPointX, StartPointY), 
            new Point(EndPointX, EndPointY));
    }
}

public class DxfDrawing : ViewModelBase
{
    public DxfDrawing()
    {
        Entities = new List<DxfEntity>();
    }

    public List<DxfEntity> Entities { get; set; }

    public void Render(DrawingContext context, Rect bounds)
    {
        using var t = context.PushTransform(Matrix.CreateTranslation(0.0, bounds.Height));
        using var s = context.PushTransform(Matrix.CreateScale(1.0, -1.0));

        context.FillRectangle(Brushes.Black, bounds);

        foreach (var entity in Entities)
        {
            entity.Render(context);
        }
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

    public void Add(DxfEntity dxfEntity)
    {
        DxfDrawing.Entities.Add(dxfEntity);
    }
    
    public void Render(DrawingContext context, Rect bounds)
    {
        DxfDrawing.Render(context, bounds);
    }

    public void Open(Stream stream)
    {
        using var reader = new StreamReader(stream);

        var dxfDrawing = DxfReaderService.ReadDxfDrawing(reader);

        DxfDrawing = dxfDrawing;

        CanvasService.Invalidate();
    }

    public void SaveAs(Stream stream)
    {
        using var writer = new StreamWriter(stream);

        DxfWriterService.WriteDxfDrawing(writer, DxfDrawing);
    }
}
