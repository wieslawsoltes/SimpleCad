using System;
using System.Collections.Generic;
using System.Linq;
using Avalonia;
using Avalonia.Input;
using SkiaSharp;
using SimpleCad.ViewModels;

namespace SimpleCad.Model;

public class SelectionTool : Tool
{
    private readonly List<DxfEntity> _selectedEntities = new();
    private bool _isDragging = false;
    private Point _dragStartPoint;
    private Point _dragCurrentPoint;
    
    public SelectionTool(IDrawingService drawingService, ICanvasService canvasService, IPanAndZoomService panAndZoomService)
        : base(drawingService, canvasService, panAndZoomService)
    {
    }

    public IReadOnlyList<DxfEntity> SelectedEntities => _selectedEntities.AsReadOnly();
    
    public bool IsDragging => _isDragging;
    
    public SKRect? GetDragRectangle()
    {
        if (!_isDragging) return null;
        
        var height = CanvasService.GetHeight();
        var startY = height - _dragStartPoint.Y;
        var endY = height - _dragCurrentPoint.Y;
        
        var minX = (float)Math.Min(_dragStartPoint.X, _dragCurrentPoint.X);
        var maxX = (float)Math.Max(_dragStartPoint.X, _dragCurrentPoint.X);
        var minY = (float)Math.Min(startY, endY);
        var maxY = (float)Math.Max(startY, endY);
        
        return new SKRect(minX, minY, maxX, maxY);
    }

    public override void OnPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        base.OnPointerPressed(sender, e);

        var position = Map(e.GetPosition(sender as Visual));
        var (x, y) = position;
        var height = CanvasService.GetHeight();
        y = height - y;

        // Check if clicking on an existing entity
        var clickedEntity = FindEntityAtPoint((float)x, (float)y);
        
        if (clickedEntity != null)
        {
            // Handle entity selection
            if (e.KeyModifiers.HasFlag(KeyModifiers.Control))
            {
                // Toggle selection with Ctrl key
                if (_selectedEntities.Contains(clickedEntity))
                {
                    _selectedEntities.Remove(clickedEntity);
                }
                else
                {
                    _selectedEntities.Add(clickedEntity);
                }
            }
            else
            {
                // Single selection (clear others)
                _selectedEntities.Clear();
                _selectedEntities.Add(clickedEntity);
            }
        }
        else
        {
            // Start drag selection if not clicking on entity
            if (!e.KeyModifiers.HasFlag(KeyModifiers.Control))
            {
                _selectedEntities.Clear();
            }
            
            _isDragging = true;
            _dragStartPoint = position;
            _dragCurrentPoint = position;
        }

        CanvasService.Invalidate();
    }

    public override void OnPointerReleased(object? sender, PointerReleasedEventArgs e)
    {
        base.OnPointerReleased(sender, e);
        
        if (_isDragging)
        {
            var position = Map(e.GetPosition(sender as Visual));
            _dragCurrentPoint = position;
            
            // Perform selection rectangle
            SelectEntitiesInRectangle(_dragStartPoint, _dragCurrentPoint);
            
            _isDragging = false;
        }

        CanvasService.Invalidate();
    }

    public override void OnPointerMoved(object? sender, PointerEventArgs e)
    {
        base.OnPointerMoved(sender, e);

        if (_isDragging)
        {
            var position = Map(e.GetPosition(sender as Visual));
            _dragCurrentPoint = position;
            CanvasService.Invalidate();
        }
    }

    private DxfEntity? FindEntityAtPoint(float x, float y)
    {
        var entities = GetAllEntities();
        
        // Search in reverse order to find topmost entity
        for (int i = entities.Count - 1; i >= 0; i--)
        {
            if (entities[i].Contains(x, y))
            {
                return entities[i];
            }
        }
        
        return null;
    }

    private void SelectEntitiesInRectangle(Point start, Point end)
    {
        var height = CanvasService.GetHeight();
        var startY = height - start.Y;
        var endY = height - end.Y;
        
        var minX = (float)Math.Min(start.X, end.X);
        var maxX = (float)Math.Max(start.X, end.X);
        var minY = (float)Math.Min(startY, endY);
        var maxY = (float)Math.Max(startY, endY);
        
        var selectionRect = new SKRect(minX, minY, maxX, maxY);
        var entities = GetAllEntities();
        
        foreach (var entity in entities)
        {
            var bounds = entity.GetBounds();
            
            // Check if entity bounds intersect with selection rectangle
            if (selectionRect.IntersectsWith(bounds) && !_selectedEntities.Contains(entity))
            {
                _selectedEntities.Add(entity);
            }
        }
    }

    private List<DxfEntity> GetAllEntities()
    {
        // Access entities through the drawing service's DxfFile
        if (DrawingService is DrawingViewModel drawingViewModel)
        {
            return drawingViewModel.DxfFile.GetEntities().ToList();
        }
        return new List<DxfEntity>();
    }

    public void ClearSelection()
    {
        _selectedEntities.Clear();
        CanvasService.Invalidate();
    }

    public void SelectAll()
    {
        _selectedEntities.Clear();
        _selectedEntities.AddRange(GetAllEntities());
        CanvasService.Invalidate();
    }

    public void DeleteSelected()
    {
        foreach (var entity in _selectedEntities.ToList())
        {
            DrawingService.Remove(entity);
        }
        _selectedEntities.Clear();
        CanvasService.Invalidate();
    }
}