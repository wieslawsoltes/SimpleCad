using System;
using Avalonia;
using Avalonia.Input;

namespace SimpleCad.Model;

public class MTextTool : Tool
{
    private DxfMText? _currentEntity;
    private Point _startPoint;
    
    public MTextTool(IDrawingService drawingService, ICanvasService canvasService, IPanAndZoomService panAndZoomService)
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
    
        SetTextBounds(_currentEntity, _startPoint, e);

        // Set default multi-line text content
        _currentEntity.TextValue = "Multi-line\nText Sample\nLine 3";
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

        SetTextBounds(_currentEntity, _startPoint, e);

        CanvasService.Invalidate();
    }

    private DxfMText Add(Point position)
    {
        var (x, y) = position;
        var height = CanvasService.GetHeight();
        y = height - y;

        var entity = new DxfMText
        {
            InsertionPointX = x,
            InsertionPointY = y,
            Height = 10.0, // Default text height
            TextValue = "MText", // Placeholder text
            ReferenceRectangleWidth = 100.0, // Default width
            AttachmentPoint = 1, // Top-left
            RotationAngle = 0.0
        };

        entity.Invalidate();

        DrawingService.Add(entity);

        return entity;
    }

    private void SetTextBounds(DxfMText entity, Point startPoint, PointerEventArgs e)
    {
        var position = Map(e.GetPosition(e.Source as Visual));
        var (x, y) = position;
        var height = CanvasService.GetHeight();
        y = height - y;
        
        var startX = startPoint.X;
        var startY = height - startPoint.Y;
        
        // Calculate the reference rectangle width based on drag distance
        var width = Math.Abs(x - startX);
        if (width < 50) width = 50; // Minimum width
        
        entity.ReferenceRectangleWidth = width;
        
        entity.Invalidate();
    }
}