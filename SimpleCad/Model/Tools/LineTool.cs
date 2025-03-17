using System;
using Avalonia;
using Avalonia.Input;

namespace SimpleCad.Model;

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
