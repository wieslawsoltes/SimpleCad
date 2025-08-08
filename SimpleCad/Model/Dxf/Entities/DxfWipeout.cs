using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using SkiaSharp;

namespace SimpleCad.Model;

public class DxfWipeout : DxfEntity
{
    private readonly List<SKPoint> _vertices = new();
    private SKPaint _fillPaint;
    private SKPaint _strokePaint;
    private SKPath? _path;
    private SKPath? _fillPath;
    private SKRect? _bounds;
    private bool _showFrame = true;
    private double _thickness = 1.0;

    public DxfWipeout()
    {
        _fillPaint = new SKPaint
        {
            Style = SKPaintStyle.Fill,
            Color = SKColors.White, // Default background color
            IsAntialias = true
        };

        _strokePaint = new SKPaint
        {
            Style = SKPaintStyle.Stroke,
            Color = Color,
            StrokeWidth = (float)_thickness,
            IsAntialias = true
        };

        AddProperty(0, "WIPEOUT");
    }

    public List<SKPoint> Vertices => _vertices;

    public SKColor FillColor
    {
        get => _fillPaint.Color;
        set
        {
            _fillPaint.Color = value;
            Invalidate();
        }
    }

    public bool ShowFrame
    {
        get => _showFrame;
        set
        {
            _showFrame = value;
            UpdateOrAddProperty(290, value ? "1" : "0");
            Invalidate();
        }
    }

    public void AddVertex(double x, double y)
    {
        _vertices.Add(new SKPoint((float)x, (float)y));
        Invalidate();
    }

    public void ClearVertices()
    {
        _vertices.Clear();
        Invalidate();
    }

    public override void Render(SKCanvas context, double zoomFactor)
    {
        if (_path is null || _vertices.Count < 3)
        {
            return;
        }

        // Render the filled area (wipeout effect)
        context.DrawPath(_path, _fillPaint);

        // Optionally render the frame/border
        if (_showFrame)
        {
            _strokePaint.StrokeWidth = (float)(_thickness / zoomFactor);
            _strokePaint.Color = Color;
            context.DrawPath(_path, _strokePaint);
        }
    }

    public override void UpdateObject()
    {
        base.UpdateObject();

        // Parse vertices from properties
        _vertices.Clear();
        
        var xCoords = Properties.Where(p => p.Code == 10).Select(p => double.Parse(p.Data.Trim(), CultureInfo.InvariantCulture)).ToList();
        var yCoords = Properties.Where(p => p.Code == 20).Select(p => double.Parse(p.Data.Trim(), CultureInfo.InvariantCulture)).ToList();
        
        for (int i = 0; i < Math.Min(xCoords.Count, yCoords.Count); i++)
        {
            _vertices.Add(new SKPoint((float)xCoords[i], (float)yCoords[i]));
        }

        // Parse show frame flag (code 290)
        if (Properties.FirstOrDefault(x => x.Code == 290) is { } showFrameProp)
        {
            _showFrame = showFrameProp.Data.Trim() == "1";
        }
    }

    public override void UpdateProperties()
    {
        base.UpdateProperties();

        // Remove existing vertex properties
        Properties.RemoveAll(p => p.Code == 10 || p.Code == 20);

        // Add vertex properties
        for (int i = 0; i < _vertices.Count; i++)
        {
            AddProperty(10, _vertices[i].X.ToString(CultureInfo.InvariantCulture));
            AddProperty(20, _vertices[i].Y.ToString(CultureInfo.InvariantCulture));
        }

        // Update show frame property
        UpdateOrAddProperty(290, _showFrame ? "1" : "0");
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

    public override void Invalidate()
    {
        _path = CreatePath();
        _fillPath = _path;
        _bounds = _path?.Bounds ?? SKRect.Empty;
    }

    public override bool Contains(float x, float y)
    {
        if (_fillPath is null)
        {
            return false;
        }

        return _fillPath.Contains(x, y);
    }

    public override SKRect GetBounds()
    {
        return _bounds ?? SKRect.Empty;
    }

    private SKPath CreatePath()
    {
        var path = new SKPath();

        if (_vertices.Count >= 3)
        {
            path.MoveTo(_vertices[0]);
            for (int i = 1; i < _vertices.Count; i++)
            {
                path.LineTo(_vertices[i]);
            }
            path.Close(); // Close the polygon
        }

        return path;
    }

    public void Dispose()
    {
        _fillPaint?.Dispose();
        _strokePaint?.Dispose();
        _path?.Dispose();
        _fillPath?.Dispose();
    }
}