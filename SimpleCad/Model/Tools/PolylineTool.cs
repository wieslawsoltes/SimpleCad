using System;
using Avalonia;
using Avalonia.Input;

namespace SimpleCad.Model;

public class PolylineTool : Tool
{
    private DxfPolyline? _currentEntity;
    private bool _isDrawing = false;
    
    public PolylineTool(IDrawingService drawingService, ICanvasService canvasService, IPanAndZoomService panAndZoomService)
        : base(drawingService, canvasService, panAndZoomService)
    {
    }

    public override void OnPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        base.OnPointerPressed(sender, e);

        var position = Map(e.GetPosition(sender as Visual));
    
        if (!_isDrawing)
        {
            // Start new polyline
            _currentEntity = Add(position);
            _isDrawing = true;
        }
        else if (_currentEntity != null)
        {
            // Add point to existing polyline
            AddPoint(_currentEntity, position);
            
            // Check for double-click or right-click to finish
            if (e.ClickCount == 2 || e.GetCurrentPoint(sender as Visual).Properties.IsRightButtonPressed)
            {
                FinishPolyline();
            }
        }

        CanvasService.Invalidate();
    }

    public override void OnPointerMoved(object? sender, PointerEventArgs e)
    {
        base.OnPointerMoved(sender, e);

        if (_currentEntity is null || !_isDrawing)
        {
            return;
        }

        // Update the last point to follow mouse cursor
        UpdateLastPoint(_currentEntity, sender, e);

        CanvasService.Invalidate();
    }

    public override void OnPointerReleased(object? sender, PointerReleasedEventArgs e)
    {
        base.OnPointerReleased(sender, e);
        
        // Handle right-click to finish polyline
        if (e.InitialPressMouseButton == MouseButton.Right && _isDrawing)
        {
            FinishPolyline();
            CanvasService.Invalidate();
        }
    }

    private DxfPolyline Add(Point position)
    {
        var (x, y) = position;
        var height = CanvasService.GetHeight();
        y = height - y;

        var entity = new DxfPolyline
        {
            IsClosed = false
        };

        // Add first point
        entity.Vertices.Add((x, y));
        // Add temporary second point (will be updated by mouse movement)
        entity.Vertices.Add((x, y));

        entity.Invalidate();

        DrawingService.Add(entity);

        return entity;
    }

    private void AddPoint(DxfPolyline entity, Point position)
    {
        var (x, y) = position;
        var height = CanvasService.GetHeight();
        y = height - y;
        
        // Update the last temporary point to the clicked position
        entity.Vertices[entity.Vertices.Count - 1] = (x, y);
        
        // Add new temporary point for next segment
        entity.Vertices.Add((x, y));
        
        entity.Invalidate();
    }

    private void UpdateLastPoint(DxfPolyline entity, object? sender, PointerEventArgs e)
    {
        var position = Map(e.GetPosition(sender as Visual));
        var (x, y) = position;
        var height = CanvasService.GetHeight();
        y = height - y;
        
        // Update the last point to follow mouse cursor
        if (entity.Vertices.Count > 0)
        {
            entity.Vertices[entity.Vertices.Count - 1] = (x, y);
        }
        
        entity.Invalidate();
    }

    private void FinishPolyline()
    {
        if (_currentEntity != null)
        {
            // Remove the last temporary point
            if (_currentEntity.Vertices.Count > 1)
            {
                _currentEntity.Vertices.RemoveAt(_currentEntity.Vertices.Count - 1);
            }
            
            _currentEntity.UpdateProperties();
            _currentEntity = null;
        }
        
        _isDrawing = false;
    }
}