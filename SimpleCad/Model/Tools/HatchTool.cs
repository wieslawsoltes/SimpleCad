using System;
using System.Collections.Generic;
using Avalonia;
using Avalonia.Input;
using SkiaSharp;

namespace SimpleCad.Model;

public class HatchTool : Tool
{
    private DxfHatch? _currentEntity;
    private List<SKPoint> _currentBoundary = new();
    private bool _isDrawing = false;
    
    public HatchTool(IDrawingService drawingService, ICanvasService canvasService, IPanAndZoomService panAndZoomService)
        : base(drawingService, canvasService, panAndZoomService)
    {
    }

    public override void OnPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        base.OnPointerPressed(sender, e);

        var position = Map(e.GetPosition(sender as Visual));
    
        if (!_isDrawing)
        {
            // Start new hatch boundary
            _currentEntity = Add(position);
            _currentBoundary.Clear();
            AddBoundaryPoint(position);
            _isDrawing = true;
        }
        else
        {
            // Add point to boundary
            AddBoundaryPoint(position);
            
            // Check for double-click or right-click to finish
            if (e.ClickCount == 2 || e.GetCurrentPoint(sender as Visual).Properties.IsRightButtonPressed)
            {
                FinishHatch();
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

        // Update preview of next boundary point
        UpdatePreview(sender, e);

        CanvasService.Invalidate();
    }

    public override void OnPointerReleased(object? sender, PointerReleasedEventArgs e)
    {
        base.OnPointerReleased(sender, e);
        
        // Handle right-click to finish hatch
        if (e.InitialPressMouseButton == MouseButton.Right && _isDrawing)
        {
            FinishHatch();
            CanvasService.Invalidate();
        }
    }

    private DxfHatch Add(Point position)
    {
        var entity = new DxfHatch
        {
            PatternName = "SOLID", // Default to solid fill
            PatternScale = 1.0,
            PatternAngle = 0.0,
            FillColor = SKColors.LightGray
        };

        entity.Invalidate();

        DrawingService.Add(entity);

        return entity;
    }

    private void AddBoundaryPoint(Point position)
    {
        var (x, y) = position;
        var height = CanvasService.GetHeight();
        y = height - y;
        
        _currentBoundary.Add(new SKPoint((float)x, (float)y));
        
        if (_currentEntity != null)
        {
            UpdateHatchBoundary();
        }
    }

    private void UpdatePreview(object? sender, PointerEventArgs e)
    {
        // This could be used to show a preview line to the next point
        // For simplicity, we'll just update the hatch boundary
        if (_currentEntity != null)
        {
            UpdateHatchBoundary();
        }
    }

    private void UpdateHatchBoundary()
    {
        if (_currentEntity == null || _currentBoundary.Count < 3)
        {
            return;
        }

        // Update the hatch entity with the current boundary
        // Note: This is a simplified implementation
        // In a real application, you'd need to properly set the boundary paths
        _currentEntity.Invalidate();
    }

    private void FinishHatch()
    {
        if (_currentEntity != null && _currentBoundary.Count >= 3)
        {
            // Close the boundary if it's not already closed
            if (_currentBoundary.Count > 2)
            {
                var first = _currentBoundary[0];
                var last = _currentBoundary[_currentBoundary.Count - 1];
                
                // If the last point is not close to the first, close the boundary
                var distance = Math.Sqrt(Math.Pow(first.X - last.X, 2) + Math.Pow(first.Y - last.Y, 2));
                if (distance > 5) // 5 pixel tolerance
                {
                    _currentBoundary.Add(first);
                }
            }
            
            _currentEntity.UpdateProperties();
        }
        
        _currentEntity = null;
        _currentBoundary.Clear();
        _isDrawing = false;
    }
}