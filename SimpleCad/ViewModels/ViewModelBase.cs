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
    private LineEntity? _currentEntity;
    
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

    private LineEntity Add(Point position)
    {
        var entity = new LineEntity
        {
            StartPointX = position.X,
            StartPointY = position.Y,
            EndPointX = position.X,
            EndPointY = position.Y,
        };

        DrawingService.Add(entity);

        return entity;
    }

    private void Move(LineEntity entity, object? sender, PointerEventArgs e)
    {
        var position = e.GetPosition(sender as Visual);

        if (e.KeyModifiers == KeyModifiers.Shift)
        {
            var deltaX = Math.Abs(position.X - entity.StartPointX);
            var deltaY = Math.Abs(position.Y - entity.StartPointY);

            if (deltaX > deltaY)
            {
                entity.EndPointX = position.X;
                entity.EndPointY = entity.StartPointY;
            }
            else
            {
                entity.EndPointX = entity.StartPointX;
                entity.EndPointY = position.Y;
            }
        }
        else
        {
            entity.EndPointX = position.X;
            entity.EndPointY = position.Y;
        }
    }
}

public abstract class Entity : ViewModelBase
{
    public abstract void Render(DrawingContext context);
}

public class LineEntity : Entity
{
    private Pen _pen;

    public LineEntity()
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

public interface IDrawingService
{
    void Add(Entity entity);
}

public interface ICanvasService
{
    void Invalidate();
}

public class DrawingViewModel : ViewModelBase, IDrawingService
{
    public DrawingViewModel(ICanvasService canvasService)
    {
        CanvasService = canvasService;
        CurrentTool = new LineTool(this, canvasService);
        Entities = new List<Entity>();
    }

    public ICanvasService CanvasService { get; }

    public Tool? CurrentTool { get; set; }

    public List<Entity> Entities { get; set; }

    public void Add(Entity entity)
    {
        Entities.Add(entity);
    }
    
    public void Render(DrawingContext context, Rect bounds)
    {
        context.FillRectangle(Brushes.Black, bounds);

        foreach (var entity in Entities)
        {
            entity.Render(context);
        }
    }

    public void SaveAs(Stream stream, double height)
    {
        using var writer = new StreamWriter(stream);

        WriteEntities(writer, height);

        WriteEof(writer);
    }

    private void WriteEntities(StreamWriter writer, double height)
    {
        writer.WriteLine("0");
        writer.WriteLine("SECTION");
        writer.WriteLine("2");
        writer.WriteLine("ENTITIES");

        foreach (var entity in Entities)
        {
            if (entity is LineEntity lineEntity)
            {
                writer.WriteLine("0");
                writer.WriteLine("LINE");

                writer.WriteLine("10");
                writer.WriteLine(lineEntity.StartPointX.ToString(CultureInfo.InvariantCulture));

                writer.WriteLine("20");
                writer.WriteLine((height - lineEntity.StartPointY).ToString(CultureInfo.InvariantCulture));

                writer.WriteLine("11");
                writer.WriteLine(lineEntity.EndPointX.ToString(CultureInfo.InvariantCulture));

                writer.WriteLine("21");
                writer.WriteLine((height - lineEntity.EndPointY).ToString(CultureInfo.InvariantCulture));
            }
        }

        writer.WriteLine("0");
        writer.WriteLine("ENDSEC");
    }

    private static void WriteEof(StreamWriter writer)
    {
        writer.WriteLine("0");
        writer.WriteLine("EOF");
    }
}
