using System;
using System.Globalization;
using System.Linq;
using SkiaSharp;

namespace SimpleCad.Model;

public class DxfTrace : DxfEntity
{
    private double _point1X, _point1Y;
    private double _point2X, _point2Y;
    private double _point3X, _point3Y;
    private double _point4X, _point4Y;
    private SKRect _bounds = SKRect.Empty;
    private bool _boundsValid = false;
    private SKPath? _path;

    public DxfTrace()
    {
        AddProperty(0, "SOLID");
    }

    public double Point1X
    {
        get => _point1X;
        set
        {
            _point1X = value;
            UpdateOrAddProperty(10, value.ToString(CultureInfo.InvariantCulture));
            Invalidate();
        }
    }

    public double Point1Y
    {
        get => _point1Y;
        set
        {
            _point1Y = value;
            UpdateOrAddProperty(20, value.ToString(CultureInfo.InvariantCulture));
            Invalidate();
        }
    }

    public double Point2X
    {
        get => _point2X;
        set
        {
            _point2X = value;
            UpdateOrAddProperty(11, value.ToString(CultureInfo.InvariantCulture));
            Invalidate();
        }
    }

    public double Point2Y
    {
        get => _point2Y;
        set
        {
            _point2Y = value;
            UpdateOrAddProperty(21, value.ToString(CultureInfo.InvariantCulture));
            Invalidate();
        }
    }

    public double Point3X
    {
        get => _point3X;
        set
        {
            _point3X = value;
            UpdateOrAddProperty(12, value.ToString(CultureInfo.InvariantCulture));
            Invalidate();
        }
    }

    public double Point3Y
    {
        get => _point3Y;
        set
        {
            _point3Y = value;
            UpdateOrAddProperty(22, value.ToString(CultureInfo.InvariantCulture));
            Invalidate();
        }
    }

    public double Point4X
    {
        get => _point4X;
        set
        {
            _point4X = value;
            UpdateOrAddProperty(13, value.ToString(CultureInfo.InvariantCulture));
            Invalidate();
        }
    }

    public double Point4Y
    {
        get => _point4Y;
        set
        {
            _point4Y = value;
            UpdateOrAddProperty(23, value.ToString(CultureInfo.InvariantCulture));
            Invalidate();
        }
    }

    public override void Render(SKCanvas context, double zoomFactor)
    {
        if (_path == null)
            return;

        using var fillPaint = new SKPaint
        {
            Color = Color,
            Style = SKPaintStyle.Fill,
            IsAntialias = true
        };

        using var strokePaint = new SKPaint
        {
            Color = Color,
            Style = SKPaintStyle.Stroke,
            StrokeWidth = (float)(1.0 / zoomFactor),
            IsAntialias = true
        };

        context.DrawPath(_path, fillPaint);
        context.DrawPath(_path, strokePaint);
    }

    public override void UpdateObject()
    {
        base.UpdateObject();
        
        foreach (var property in Properties)
        {
            switch (property.Code)
            {
                case 10: // Point 1 X
                    if (double.TryParse(property.Data, NumberStyles.Float, CultureInfo.InvariantCulture, out var p1X))
                        _point1X = p1X;
                    break;
                case 20: // Point 1 Y
                    if (double.TryParse(property.Data, NumberStyles.Float, CultureInfo.InvariantCulture, out var p1Y))
                        _point1Y = p1Y;
                    break;
                case 11: // Point 2 X
                    if (double.TryParse(property.Data, NumberStyles.Float, CultureInfo.InvariantCulture, out var p2X))
                        _point2X = p2X;
                    break;
                case 21: // Point 2 Y
                    if (double.TryParse(property.Data, NumberStyles.Float, CultureInfo.InvariantCulture, out var p2Y))
                        _point2Y = p2Y;
                    break;
                case 12: // Point 3 X
                    if (double.TryParse(property.Data, NumberStyles.Float, CultureInfo.InvariantCulture, out var p3X))
                        _point3X = p3X;
                    break;
                case 22: // Point 3 Y
                    if (double.TryParse(property.Data, NumberStyles.Float, CultureInfo.InvariantCulture, out var p3Y))
                        _point3Y = p3Y;
                    break;
                case 13: // Point 4 X
                    if (double.TryParse(property.Data, NumberStyles.Float, CultureInfo.InvariantCulture, out var p4X))
                        _point4X = p4X;
                    break;
                case 23: // Point 4 Y
                    if (double.TryParse(property.Data, NumberStyles.Float, CultureInfo.InvariantCulture, out var p4Y))
                        _point4Y = p4Y;
                    break;
            }
        }
        
        Invalidate();
    }

    public override void UpdateProperties()
    {
        base.UpdateProperties();
        
        UpdateOrAddProperty(10, _point1X.ToString(CultureInfo.InvariantCulture));
        UpdateOrAddProperty(20, _point1Y.ToString(CultureInfo.InvariantCulture));
        UpdateOrAddProperty(11, _point2X.ToString(CultureInfo.InvariantCulture));
        UpdateOrAddProperty(21, _point2Y.ToString(CultureInfo.InvariantCulture));
        UpdateOrAddProperty(12, _point3X.ToString(CultureInfo.InvariantCulture));
        UpdateOrAddProperty(22, _point3Y.ToString(CultureInfo.InvariantCulture));
        UpdateOrAddProperty(13, _point4X.ToString(CultureInfo.InvariantCulture));
        UpdateOrAddProperty(23, _point4Y.ToString(CultureInfo.InvariantCulture));
    }

    public override void Invalidate()
    {
        _boundsValid = false;
        CreatePath();
    }

    public override bool Contains(float x, float y)
    {
        if (_path == null)
            return false;
            
        return _path.Contains(x, y);
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

    private void CreatePath()
    {
        _path?.Dispose();
        _path = new SKPath();
        
        // Create a quadrilateral path
        _path.MoveTo((float)_point1X, (float)_point1Y);
        _path.LineTo((float)_point2X, (float)_point2Y);
        _path.LineTo((float)_point3X, (float)_point3Y);
        _path.LineTo((float)_point4X, (float)_point4Y);
        _path.Close();
    }

    private SKRect CalculateBounds()
    {
        var points = new[]
        {
            new SKPoint((float)_point1X, (float)_point1Y),
            new SKPoint((float)_point2X, (float)_point2Y),
            new SKPoint((float)_point3X, (float)_point3Y),
            new SKPoint((float)_point4X, (float)_point4Y)
        };

        if (points.Length == 0)
            return SKRect.Empty;

        var minX = points.Min(p => p.X);
        var minY = points.Min(p => p.Y);
        var maxX = points.Max(p => p.X);
        var maxY = points.Max(p => p.Y);

        return new SKRect(minX, minY, maxX, maxY);
    }
}
