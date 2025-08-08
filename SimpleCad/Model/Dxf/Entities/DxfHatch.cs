using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace SimpleCad.Model;

public class DxfHatch : DxfEntity
{
    private readonly List<List<SKPoint>> _boundaryPaths = new();
    private string _patternName = "SOLID";
    private double _patternScale = 1.0;
    private double _patternAngle = 0.0;
    private bool _isSolid = true;
    private SKColor _fillColor = SKColors.Gray;
    private SKRect _bounds = SKRect.Empty;
    private bool _boundsValid = false;
    private SKPath? _hatchPath;

    public DxfHatch()
    {
        AddProperty(0, "HATCH");
    }

    public string PatternName
    {
        get => _patternName;
        set
        {
            _patternName = value;
            _isSolid = string.Equals(value, "SOLID", StringComparison.OrdinalIgnoreCase);
            UpdateOrAddProperty(2, value);
            Invalidate();
        }
    }

    public double PatternScale
    {
        get => _patternScale;
        set
        {
            _patternScale = value;
            UpdateOrAddProperty(41, value.ToString(CultureInfo.InvariantCulture));
            Invalidate();
        }
    }

    public double PatternAngle
    {
        get => _patternAngle;
        set
        {
            _patternAngle = value;
            UpdateOrAddProperty(52, value.ToString(CultureInfo.InvariantCulture));
            Invalidate();
        }
    }

    public SKColor FillColor
    {
        get => _fillColor;
        set
        {
            _fillColor = value;
            Invalidate();
        }
    }

    public void AddBoundaryPath(List<SKPoint> path)
    {
        _boundaryPaths.Add(new List<SKPoint>(path));
        Invalidate();
    }

    public void ClearBoundaryPaths()
    {
        _boundaryPaths.Clear();
        Invalidate();
    }

    public override void Render(SKCanvas context, double zoomFactor)
    {
        if (_boundaryPaths.Count == 0) return;

        CreateHatchPath();
        if (_hatchPath == null) return;

        if (_isSolid)
        {
            // Render solid fill
            using var fillPaint = new SKPaint
            {
                Color = _fillColor,
                Style = SKPaintStyle.Fill,
                IsAntialias = true
            };
            
            context.DrawPath(_hatchPath, fillPaint);
        }
        else
        {
            // Render pattern fill (simplified - just outline for now)
            using var strokePaint = new SKPaint
            {
                Color = Color,
                Style = SKPaintStyle.Stroke,
                StrokeWidth = (float)(1.0 / zoomFactor),
                IsAntialias = true
            };
            
            context.DrawPath(_hatchPath, strokePaint);
        }
    }

    public override void UpdateObject()
    {
        base.UpdateObject();
        
        var currentBoundary = new List<SKPoint>();
        
        foreach (var property in Properties)
        {
            switch (property.Code)
            {
                case 2: // Pattern name
                    _patternName = property.Data;
                    _isSolid = string.Equals(_patternName, "SOLID", StringComparison.OrdinalIgnoreCase);
                    break;
                case 10: // Boundary path X coordinate
                    if (double.TryParse(property.Data, NumberStyles.Float, CultureInfo.InvariantCulture, out var x))
                    {
                        // Store X coordinate temporarily
                        if (currentBoundary.Count == 0 || currentBoundary.Last().X != float.MinValue)
                        {
                            currentBoundary.Add(new SKPoint((float)x, float.MinValue));
                        }
                        else
                        {
                            var lastPoint = currentBoundary.Last();
                            currentBoundary[currentBoundary.Count - 1] = new SKPoint((float)x, lastPoint.Y);
                        }
                    }
                    break;
                case 20: // Boundary path Y coordinate
                    if (double.TryParse(property.Data, NumberStyles.Float, CultureInfo.InvariantCulture, out var y))
                    {
                        if (currentBoundary.Count > 0)
                        {
                            var lastPoint = currentBoundary.Last();
                            if (lastPoint.Y == float.MinValue)
                            {
                                currentBoundary[currentBoundary.Count - 1] = new SKPoint(lastPoint.X, (float)y);
                            }
                        }
                    }
                    break;
                case 41: // Pattern scale
                    if (double.TryParse(property.Data, NumberStyles.Float, CultureInfo.InvariantCulture, out var scale))
                        _patternScale = scale;
                    break;
                case 52: // Pattern angle
                    if (double.TryParse(property.Data, NumberStyles.Float, CultureInfo.InvariantCulture, out var angle))
                        _patternAngle = angle;
                    break;
                case 91: // Number of boundary paths (end of current boundary)
                    if (currentBoundary.Count > 0)
                    {
                        // Filter out incomplete points
                        var validPoints = currentBoundary.Where(p => p.Y != float.MinValue).ToList();
                        if (validPoints.Count > 2)
                        {
                            _boundaryPaths.Add(validPoints);
                        }
                        currentBoundary.Clear();
                    }
                    break;
            }
        }
        
        // Add any remaining boundary
        if (currentBoundary.Count > 0)
        {
            var validPoints = currentBoundary.Where(p => p.Y != float.MinValue).ToList();
            if (validPoints.Count > 2)
            {
                _boundaryPaths.Add(validPoints);
            }
        }
        
        Invalidate();
    }

    public override void UpdateProperties()
    {
        base.UpdateProperties();
        
        UpdateOrAddProperty(2, _patternName);
        UpdateOrAddProperty(41, _patternScale.ToString(CultureInfo.InvariantCulture));
        UpdateOrAddProperty(52, _patternAngle.ToString(CultureInfo.InvariantCulture));
        
        // Add boundary path coordinates
        foreach (var boundary in _boundaryPaths)
        {
            foreach (var point in boundary)
            {
                AddProperty(10, point.X.ToString(CultureInfo.InvariantCulture));
                AddProperty(20, point.Y.ToString(CultureInfo.InvariantCulture));
            }
        }
    }

    public override void Invalidate()
    {
        _boundsValid = false;
        _hatchPath?.Dispose();
        _hatchPath = null;
    }

    public override bool Contains(float x, float y)
    {
        CreateHatchPath();
        if (_hatchPath == null) return false;
        
        return _hatchPath.Contains(x, y);
    }

    public override SKRect GetBounds()
    {
        if (_boundsValid)
            return _bounds;
            
        _bounds = CalculateBounds();
        _boundsValid = true;
        return _bounds;
    }

    private void UpdateOrAddProperty(int code, string value)
    {
        var existingProperty = Properties.FirstOrDefault(p => p.Code == code);
        if (existingProperty != null)
        {
            existingProperty.Data = value;
        }
        else
        {
            AddProperty(code, value);
        }
    }

    private void CreateHatchPath()
    {
        if (_hatchPath != null || _boundaryPaths.Count == 0) return;
        
        _hatchPath = new SKPath();
        
        foreach (var boundary in _boundaryPaths)
        {
            if (boundary.Count < 3) continue;
            
            _hatchPath.MoveTo(boundary[0]);
            for (int i = 1; i < boundary.Count; i++)
            {
                _hatchPath.LineTo(boundary[i]);
            }
            _hatchPath.Close();
        }
    }

    private SKRect CalculateBounds()
    {
        if (_boundaryPaths.Count == 0)
            return SKRect.Empty;

        var minX = float.MaxValue;
        var minY = float.MaxValue;
        var maxX = float.MinValue;
        var maxY = float.MinValue;

        foreach (var boundary in _boundaryPaths)
        {
            foreach (var point in boundary)
            {
                minX = Math.Min(minX, point.X);
                minY = Math.Min(minY, point.Y);
                maxX = Math.Max(maxX, point.X);
                maxY = Math.Max(maxY, point.Y);
            }
        }

        if (minX == float.MaxValue)
            return SKRect.Empty;

        return new SKRect(minX, minY, maxX, maxY);
    }


}
