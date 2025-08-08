using System;
using Avalonia;
using Avalonia.Input;

namespace SimpleCad.Model;

public class RectangleTool : Tool
{
    private DxfPolyline? _currentEntity;
    private Point _startPoint;
    
    public RectangleTool(IDrawingService drawingService, ICanvasService canvasService, IPanAndZoomService panAndZoomService)
        : base(drawingService, canvasService, panAndZoomService)
    {
    }

    public override void OnPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        base.OnPointerPressed(sender, e);

        var position = Map(e.GetPosition(sender as Visual));
        _startPoint = position;
    
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

    private DxfPolyline Add(Point position)
    {
        var (x, y) = position;
        var height = CanvasService.GetHeight();
        y = height - y;

        var entity = new DxfPolyline
        {
            IsClosed = true
        };

        // Add initial rectangle vertices (all at start point initially)
        entity.Vertices.Add((x, y));
        entity.Vertices.Add((x, y));
        entity.Vertices.Add((x, y));
        entity.Vertices.Add((x, y));

        entity.Invalidate();

        DrawingService.Add(entity);

        return entity;
    }

    private void Move(DxfPolyline entity, object? sender, PointerEventArgs e)
    {
        var position = Map(e.GetPosition(sender as Visual));

        var (x, y) = position;
        var height = CanvasService.GetHeight();
        y = height - y;
        
        var startX = _startPoint.X;
        var startY = height - _startPoint.Y;
        
        if (e.KeyModifiers == KeyModifiers.Shift)
        {
            // Create a square
            var deltaX = x - startX;
            var deltaY = y - startY;
            var size = Math.Max(Math.Abs(deltaX), Math.Abs(deltaY));
            
            var endX = startX + (deltaX >= 0 ? size : -size);
            var endY = startY + (deltaY >= 0 ? size : -size);
            
            entity.Vertices[0] = (startX, startY);
            entity.Vertices[1] = (endX, startY);
            entity.Vertices[2] = (endX, endY);
            entity.Vertices[3] = (startX, endY);
        }
        else
        {
            // Create a rectangle
            entity.Vertices[0] = (startX, startY);
            entity.Vertices[1] = (x, startY);
            entity.Vertices[2] = (x, y);
            entity.Vertices[3] = (startX, y);
        }
        
        entity.Invalidate();
    }
}