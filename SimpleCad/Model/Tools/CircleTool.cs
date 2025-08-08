using System;
using Avalonia;
using Avalonia.Input;

namespace SimpleCad.Model;

public class CircleTool : Tool
{
    private DxfCircle? _currentEntity;
    
    public CircleTool(IDrawingService drawingService, ICanvasService canvasService, IPanAndZoomService panAndZoomService)
        : base(drawingService, canvasService, panAndZoomService)
    {
    }

    public override void OnPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        base.OnPointerPressed(sender, e);

        var position = Map(e.GetPosition(sender as Visual));
    
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

        _currentEntity.UpdateProperties();

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

    private DxfCircle Add(Point position)
    {
        var (x, y) = position;
        var height = CanvasService.GetHeight();
        y = height - y;

        var entity = new DxfCircle
        {
            CenterX = x,
            CenterY = y,
            Radius = 0,
        };

        entity.Invalidate();

        DrawingService.Add(entity);

        return entity;
    }

    private void Move(DxfCircle entity, object? sender, PointerEventArgs e)
    {
        var position = Map(e.GetPosition(sender as Visual));

        var (x, y) = position;
        var height = CanvasService.GetHeight();
        y = height - y;
        
        var deltaX = x - entity.CenterX;
        var deltaY = y - entity.CenterY;
        var radius = Math.Sqrt(deltaX * deltaX + deltaY * deltaY);
        
        entity.Radius = radius;
        
        entity.Invalidate();
    }
}