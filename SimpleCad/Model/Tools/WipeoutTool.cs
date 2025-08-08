using System;
using System.Collections.Generic;
using Avalonia;
using Avalonia.Input;
using SkiaSharp;

namespace SimpleCad.Model;

public class WipeoutTool : Tool
{
    private DxfWipeout? _currentEntity;
    private List<SKPoint> _currentVertices = new();
    private bool _isDrawing = false;
    
    public WipeoutTool(IDrawingService drawingService, ICanvasService canvasService, IPanAndZoomService panAndZoomService)
        : base(drawingService, canvasService, panAndZoomService)
    {
    }

    public override void OnPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        base.OnPointerPressed(sender, e);

        var position = Map(e.GetPosition(sender as Visual));
    
        if (!_isDrawing)
        {
            // Start new wipeout
            _currentEntity = Add(position);
            _currentVertices.Clear();
            AddVertex(position);
            _isDrawing = true;
        }
        else
        {
            // Add vertex to wipeout
            AddVertex(position);
            
            // Check for double-click or right-click to finish
            if (e.ClickCount == 2 || e.GetCurrentPoint(sender as Visual).Properties.IsRightButtonPressed)
            {
                FinishWipeout();
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

        // Update preview of next vertex
        UpdatePreview(sender, e);

        CanvasService.Invalidate();
    }

    public override void OnPointerReleased(object? sender, PointerReleasedEventArgs e)
    {
        base.OnPointerReleased(sender, e);
        
        // Handle right-click to finish wipeout
        if (e.InitialPressMouseButton == MouseButton.Right && _isDrawing)
        {
            FinishWipeout();
            CanvasService.Invalidate();
        }
    }

    private DxfWipeout Add(Point position)
    {
        var entity = new DxfWipeout
        {
            FillColor = SKColors.White, // Default background color
            ShowFrame = true // Show frame by default for visibility during creation
        };

        entity.Invalidate();

        DrawingService.Add(entity);

        return entity;
    }

    private void AddVertex(Point position)
    {
        var (x, y) = position;
        var height = CanvasService.GetHeight();
        y = height - y;
        
        _currentVertices.Add(new SKPoint((float)x, (float)y));
        
        if (_currentEntity != null)
        {
            _currentEntity.AddVertex(x, y);
        }
    }

    private void UpdatePreview(object? sender, PointerEventArgs e)
    {
        // This could be used to show a preview line to the next vertex
        // For now, we'll just ensure the wipeout is updated
        if (_currentEntity != null)
        {
            _currentEntity.Invalidate();
        }
    }

    private void FinishWipeout()
    {
        if (_currentEntity != null && _currentVertices.Count >= 3)
        {
            // Ensure the wipeout is properly closed
            _currentEntity.UpdateProperties();
            _currentEntity.Invalidate();
        }
        else if (_currentEntity != null && _currentVertices.Count < 3)
        {
            // Remove incomplete wipeout
            DrawingService.Remove(_currentEntity);
        }
        
        _currentEntity = null;
        _currentVertices.Clear();
        _isDrawing = false;
    }

}