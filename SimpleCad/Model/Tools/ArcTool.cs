using System;
using Avalonia;
using Avalonia.Input;

namespace SimpleCad.Model;

public class ArcTool : Tool
{
    private DxfArc? _currentEntity;
    private Point _centerPoint;
    private bool _settingRadius = true;
    
    public ArcTool(IDrawingService drawingService, ICanvasService canvasService, IPanAndZoomService panAndZoomService)
        : base(drawingService, canvasService, panAndZoomService)
    {
    }

    public override void OnPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        base.OnPointerPressed(sender, e);

        var position = Map(e.GetPosition(sender as Visual));
    
        if (_currentEntity is null)
        {
            // First click: set center point
            _centerPoint = position;
            _currentEntity = Add(position);
            _settingRadius = true;
        }
        else if (_settingRadius)
        {
            // Second click: set radius and start setting angles
            SetRadius(_currentEntity, position);
            _settingRadius = false;
        }
        else
        {
            // Third click: finish the arc
            SetEndAngle(_currentEntity, position);
            _currentEntity.UpdateProperties();
            _currentEntity = null;
            _settingRadius = true;
        }

        CanvasService.Invalidate();
    }

    public override void OnPointerMoved(object? sender, PointerEventArgs e)
    {
        base.OnPointerMoved(sender, e);

        if (_currentEntity is null)
        {
            return;
        }

        var position = Map(e.GetPosition(sender as Visual));

        if (_settingRadius)
        {
            SetRadius(_currentEntity, position);
        }
        else
        {
            SetEndAngle(_currentEntity, position);
        }

        CanvasService.Invalidate();
    }

    private DxfArc Add(Point position)
    {
        var (x, y) = position;
        var height = CanvasService.GetHeight();
        y = height - y;

        var entity = new DxfArc
        {
            CenterX = x,
            CenterY = y,
            Radius = 0,
            StartAngle = 0,
            EndAngle = 90
        };

        entity.Invalidate();

        DrawingService.Add(entity);

        return entity;
    }

    private void SetRadius(DxfArc entity, Point position)
    {
        var (x, y) = position;
        var height = CanvasService.GetHeight();
        y = height - y;
        
        var deltaX = x - entity.CenterX;
        var deltaY = y - entity.CenterY;
        var radius = Math.Sqrt(deltaX * deltaX + deltaY * deltaY);
        
        entity.Radius = radius;
        
        // Set start angle based on current mouse position
        var startAngle = Math.Atan2(deltaY, deltaX) * 180.0 / Math.PI;
        entity.StartAngle = startAngle;
        entity.EndAngle = startAngle + 90; // Default 90-degree arc
        
        entity.Invalidate();
    }

    private void SetEndAngle(DxfArc entity, Point position)
    {
        var (x, y) = position;
        var height = CanvasService.GetHeight();
        y = height - y;
        
        var deltaX = x - entity.CenterX;
        var deltaY = y - entity.CenterY;
        var endAngle = Math.Atan2(deltaY, deltaX) * 180.0 / Math.PI;
        
        entity.EndAngle = endAngle;
        
        entity.Invalidate();
    }
}