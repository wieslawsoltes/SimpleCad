using System;
using Avalonia;
using Avalonia.Input;
using SkiaSharp;

namespace SimpleCad.Model;

public class PanAndZoomService
{
    private const double _baseZoomFactor = 1.15;
    private const int _minZoomLevel = -20;
    private const int _maxZoomLevel = 40;
    private int _currentZoomLevel;
    private double _zoomFactor = 1.0;
    private SKMatrix _transform = SKMatrix.Identity;
    private bool _isPanning;
    private Point _lastPanPosition;

    public SKMatrix Transform => _transform;

    public bool TryStartPan(object? sender, PointerPressedEventArgs e)
    {
        if (!e.GetCurrentPoint(null).Properties.IsMiddleButtonPressed)
        {
            return false;
        }

        _isPanning = true;
        _lastPanPosition = e.GetPosition(sender as Visual);
        e.Handled = true;

        return true;
    }

    public bool TryEndPan(PointerReleasedEventArgs e)
    {
        if (!_isPanning)
        {
            return false;
        }
        
        _isPanning = false;
        e.Handled = true;

        return true;
    }

    public bool TryMovePan(object? sender, PointerEventArgs e)
    {
        if (!_isPanning)
        {
            return false;
        }

        var position = e.GetPosition(sender as Visual);
        var delta = position - _lastPanPosition;

        _transform = SKMatrix.Concat(SKMatrix.CreateTranslation((float)delta.X, (float)delta.Y), _transform);
  
        _lastPanPosition = position;

        e.Handled = true;

        return true;
    }

    public bool Zoom(object? sender, PointerWheelEventArgs e)
    {
        var position = e.GetPosition(sender as Visual);
        var zoomDelta = e.Delta.Y > 0 ? 1 : -1;

        var newZoomLevel = Math.Clamp(_currentZoomLevel + zoomDelta, _minZoomLevel, _maxZoomLevel);
        if (newZoomLevel == _currentZoomLevel)
        {
            return false;
        }

        var oldZoomFactor = _zoomFactor;
        _currentZoomLevel = newZoomLevel;
        _zoomFactor = Math.Pow(_baseZoomFactor, _currentZoomLevel);

        var scaleFactor = _zoomFactor / oldZoomFactor;

        _transform = SKMatrix.Concat(SKMatrix.CreateTranslation((float)-position.X, (float)-position.Y), _transform);
        _transform = SKMatrix.Concat(SKMatrix.CreateScale((float)scaleFactor, (float)scaleFactor), _transform);
        _transform = SKMatrix.Concat(SKMatrix.CreateTranslation((float)position.X, (float)position.Y), _transform);
        
        return true;
    }
}
