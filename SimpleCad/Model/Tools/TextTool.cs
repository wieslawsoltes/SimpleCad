using System;
using Avalonia;
using Avalonia.Input;

namespace SimpleCad.Model;

public class TextTool : Tool
{
    private DxfText? _currentEntity;
    
    public TextTool(IDrawingService drawingService, ICanvasService canvasService, IPanAndZoomService panAndZoomService)
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

        // For text tool, we could implement a text input dialog here
        // For now, we'll just set a default text
        _currentEntity.TextValue = "Sample Text";
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

        // Text doesn't need to follow mouse movement after placement
        // Could implement rotation or scaling here if needed
    }

    private DxfText Add(Point position)
    {
        var (x, y) = position;
        var height = CanvasService.GetHeight();
        y = height - y;

        var entity = new DxfText
        {
            InsertionPointX = x,
            InsertionPointY = y,
            Height = 10.0, // Default text height
            TextValue = "Text", // Placeholder text
            RotationAngle = 0.0,
            HorizontalAlignment = 0,
            VerticalAlignment = 0
        };

        entity.Invalidate();

        DrawingService.Add(entity);

        return entity;
    }
}