using System;
using Avalonia;
using Avalonia.Input;

namespace SimpleCad.Model;

public class EllipseTool : Tool
{
    private DxfEllipse? _currentEntity;
    
    public EllipseTool(IDrawingService drawingService, ICanvasService canvasService, IPanAndZoomService panAndZoomService)
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

    private DxfEllipse Add(Point position)
    {
        var (x, y) = position;
        var height = CanvasService.GetHeight();
        y = height - y;

        var entity = new DxfEllipse
        {
            CenterX = x,
            CenterY = y,
            MajorAxisEndpointX = x,
            MajorAxisEndpointY = y,
            MinorToMajorRatio = 0.5,
        };

        entity.Invalidate();

        DrawingService.Add(entity);

        return entity;
    }

    private void Move(DxfEllipse entity, object? sender, PointerEventArgs e)
    {
        var position = Map(e.GetPosition(sender as Visual));

        var (x, y) = position;
        var height = CanvasService.GetHeight();
        y = height - y;
        
        var deltaX = x - entity.CenterX;
        var deltaY = y - entity.CenterY;
        
        if (e.KeyModifiers == KeyModifiers.Shift)
        {
            // Create a circle (equal major and minor axes)
            entity.MajorAxisEndpointX = entity.CenterX + deltaX;
            entity.MajorAxisEndpointY = entity.CenterY;
            entity.MinorToMajorRatio = 1.0;
        }
        else
        {
            // Create an ellipse
            entity.MajorAxisEndpointX = entity.CenterX + deltaX;
            entity.MajorAxisEndpointY = entity.CenterY + deltaY;
            entity.MinorToMajorRatio = 0.5;
        }
        
        entity.Invalidate();
    }
}